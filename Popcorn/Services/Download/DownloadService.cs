using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ltnet;
using NLog;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Enums;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Bandwidth;
using Popcorn.Models.Media;
using Popcorn.Services.Cache;
using Popcorn.Exceptions;

namespace Popcorn.Services.Download
{
    /// <summary>
    /// Generic download service for torrent download
    /// </summary>
    /// <typeparam name="T"><see cref="IMediaFile"/></typeparam>
    public class DownloadService<T> : IDownloadService<T> where T : IMediaFile
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private readonly ICacheService _cacheService;

        protected DownloadService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        /// <summary>
        /// Action to execute when a movie has been buffered
        /// </summary>
        /// <param name="media"><see cref="IMediaFile"/></param>
        /// <param name="reportDownloadProgress">Download progress</param>
        /// <param name="reportDownloadRate">The download rate</param>
        /// <param name="playingProgress">The playing progress</param>
        protected virtual void BroadcastMediaBuffered(T media, Progress<double> reportDownloadProgress,
            Progress<BandwidthRate> reportDownloadRate, IProgress<double> playingProgress)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Download a torrent
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task Download(T media, TorrentType torrentType, MediaType mediaType, string torrentPath,
            int uploadLimit, int downloadLimit, IProgress<double> downloadProgress,
            IProgress<BandwidthRate> bandwidthRate, IProgress<int> nbSeeds, IProgress<int> nbPeers, Action buffered,
            Action cancelled,
            CancellationTokenSource cts)
        {
            return Task.Run(async () =>
            {
                Logger.Info(
                    $"Start downloading : {torrentPath}");
                using var session = new Session();
                downloadProgress.Report(0d);
                bandwidthRate.Report(new BandwidthRate
                {
                    DownloadRate = 0d,
                    UploadRate = 0d
                });
                nbSeeds.Report(0);
                nbPeers.Report(0);
                string savePath = string.Empty;
                switch (mediaType)
                {
                    case MediaType.Movie:
                        savePath = _cacheService.MovieDownloads;
                        break;
                    case MediaType.Show:
                        savePath = _cacheService.ShowDownloads;
                        break;
                    case MediaType.Unkown:
                        savePath = _cacheService.DropFilesDownloads;
                        break;
                }

                if (torrentType == TorrentType.File)
                {
                    using var addParams = new AddTorrentParams
                    {
                        save_path = savePath,
                        ti = new TorrentInfo(torrentPath)
                    };
                    using var handle = session.add_torrent(addParams);
                    await HandleDownload(media, savePath, mediaType, uploadLimit, downloadLimit,
                        downloadProgress,
                        bandwidthRate, nbSeeds, nbPeers, handle, session, buffered, cancelled, cts);
                }
                else
                {
                    var magnet = new MagnetUri();
                    using var error = new ErrorCode();
                    var addParams = new AddTorrentParams
                    {
                        save_path = savePath,
                    };
                    magnet.parse_magnet_uri(torrentPath, addParams, error);
                    using var handle = session.add_torrent(addParams);
                    await HandleDownload(media, savePath, mediaType, uploadLimit, downloadLimit,
                        downloadProgress,
                        bandwidthRate, nbSeeds, nbPeers, handle, session, buffered, cancelled, cts);
                }
            });
        }

        /// <summary>
        /// Download media
        /// </summary>
        /// <param name="media">Media file <see cref="IMediaFile"/></param>
        /// <param name="savePath">Save path of the media</param>
        /// <param name="type">Media type <see cref="MediaType"/></param>
        /// <param name="uploadLimit">Upload limit</param>
        /// <param name="downloadLimit">Download limit</param>
        /// <param name="downloadProgress">Download progress</param>
        /// <param name="bandwidthRate">Download rate</param>
        /// <param name="nbSeeds">Number of seeders</param>
        /// <param name="nbPeers">Number of peers</param>
        /// <param name="handle"><see cref="TorrentHandle"/></param>
        /// <param name="session"><see cref="Session"/></param>
        /// <param name="buffered">Action to execute when media has been buffered</param>
        /// <param name="cancelled">Action to execute when media download has been cancelled</param>
        /// <param name="cts"><see cref="CancellationTokenSource"/></param>
        /// <returns><see cref="Task"/></returns>
        private async Task HandleDownload(T media, string savePath, MediaType type, int uploadLimit, int downloadLimit,
            IProgress<double> downloadProgress, IProgress<BandwidthRate> bandwidthRate, IProgress<int> nbSeeds,
            IProgress<int> nbPeers,
            TorrentHandle handle,
            Session session, Action buffered, Action cancelled, CancellationTokenSource cts)
        {
            handle.set_upload_limit(uploadLimit * 1024);
            handle.set_download_limit(downloadLimit * 1024);
            handle.set_sequential_download(true);
            var alreadyBuffered = false;
            var bandwidth = new Progress<BandwidthRate>();
            var prog = new Progress<double>();
            var playingProgress = new Progress<double>();
            var sw = new Stopwatch();
            sw.Start();
            var mediaIndex = -1;
            long maxSize = 0;
            var filePath = string.Empty;
            using var torrentFile = handle.torrent_file();
            var torrentStorage = torrentFile.files();
            while (!cts.IsCancellationRequested)
            {
                using var status = handle.status();
                var progress = 0d;
                if (status.has_metadata)
                {
                    var totalSizeExceptIgnoredFiles = torrentFile.total_size();
                    while (mediaIndex == -1 || string.IsNullOrEmpty(filePath))
                    {
                        var numFiles = torrentStorage.num_files();
                        for (var i = 0; i < numFiles; i++)
                        {
                            var currentSize = torrentStorage.file_size(i);
                            if (currentSize > maxSize)
                            {
                                maxSize = currentSize;
                                mediaIndex = i;
                            }
                        }

                        for (var i = 0; i < numFiles; i++)
                        {
                            if (i != mediaIndex)
                            {
                                handle.file_priority(i, 0);
                                totalSizeExceptIgnoredFiles -= torrentStorage.file_size(i);
                            }
                        }

                        var fullPath = torrentStorage.file_path(mediaIndex, savePath);
                        var shortPath = Directory.GetParent(fullPath).FullName.GetShortPath();
                        if (!string.IsNullOrEmpty(shortPath))
                        {
                            filePath = Path.Combine(shortPath, torrentStorage.file_name(mediaIndex));
                            break;
                        }
                    }

                    var fileProgressInBytes = handle.file_progress(1)[mediaIndex];
                    progress = (double) fileProgressInBytes / (double) totalSizeExceptIgnoredFiles * 100d;
                    var downRate = Math.Round(status.download_rate / 1024d, 0);
                    var upRate = Math.Round(status.upload_rate / 1024d, 0);
                    nbSeeds.Report(status.num_seeds);
                    nbPeers.Report(status.num_peers);
                    downloadProgress.Report(progress);
                    var eta = sw.GetEta(fileProgressInBytes, totalSizeExceptIgnoredFiles);
                    bandwidthRate.Report(new BandwidthRate
                    {
                        DownloadRate = downRate,
                        UploadRate = upRate,
                        ETA = eta
                    });

                    ((IProgress<double>) prog).Report(progress);
                    ((IProgress<BandwidthRate>) bandwidth).Report(new BandwidthRate
                    {
                        DownloadRate = downRate,
                        UploadRate = upRate,
                        ETA = eta
                    });
                }

                double minimumBuffering;
                switch (type)
                {
                    case MediaType.Show:
                        minimumBuffering = Constants.MinimumShowBuffering;
                        break;
                    default:
                        minimumBuffering = Constants.MinimumMovieBuffering;
                        break;
                }

                if (mediaIndex != -1 && progress >= minimumBuffering && !alreadyBuffered)
                {
                    buffered.Invoke();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        alreadyBuffered = true;
                        media.FilePath = filePath;
                        BroadcastMediaBuffered(media, prog, bandwidth, playingProgress);
                    }

                    if (!alreadyBuffered)
                    {
                        session.remove_torrent(handle);
                        if (type == MediaType.Unkown)
                        {
                            Messenger.Default.Send(
                                new UnhandledExceptionMessage(
                                    new PopcornException(
                                        LocalizationProviderHelper.GetLocalizedValue<string>(
                                            "NoMediaInDroppedTorrent"))));
                        }
                        else
                        {
                            Messenger.Default.Send(
                                new UnhandledExceptionMessage(
                                    new PopcornException(
                                        LocalizationProviderHelper.GetLocalizedValue<string>("NoMediaInTorrent"))));
                        }

                        break;
                    }
                }

                try
                {
                    await Task.Delay(1000, cts.Token);
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is ObjectDisposedException)
                {
                    cancelled.Invoke();
                    sw.Stop();
                    try
                    {
                        session.remove_torrent(handle);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    break;
                }
            }
        }
    }
}