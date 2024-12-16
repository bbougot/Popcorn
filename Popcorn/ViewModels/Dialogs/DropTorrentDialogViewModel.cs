using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Enums;
using Popcorn.Messaging;
using Popcorn.Models.Bandwidth;
using Popcorn.Models.Media;
using Popcorn.Services.Cache;
using Popcorn.Services.Download;
using Popcorn.Utils;

namespace Popcorn.ViewModels.Dialogs
{
    public class DropTorrentDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private readonly IDownloadService<MediaFile> _downloadService;

        private string _torrentPath;

        private double _downloadProgress;

        private double _downloadRate;

        private int _nbPeers;

        private int _nbSeeders;

        private CancellationTokenSource CancellationDownloadingToken { get; set; }

        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set => Set(ref _cancelCommand, value);
        }

        public string TorrentPath
        {
            get => _torrentPath;
            set => Set(ref _torrentPath, value);
        }

        public double DownloadProgress
        {
            get => _downloadProgress;
            set => Set(ref _downloadProgress, value);
        }

        public double DownloadRate
        {
            get => _downloadRate;
            set => Set(ref _downloadRate, value);
        }

        public int NbPeers
        {
            get => _nbPeers;
            set => Set(ref _nbPeers, value);
        }

        public int NbSeeders
        {
            get => _nbSeeders;
            set => Set(ref _nbSeeders, value);
        }

        /// <summary>
        /// Initialize a new instance of DropTorrentDialogViewModel
        /// </summary>
        /// <param name="cacheService">The cache service</param>
        /// <param name="torrentPath">The torrent path</param>
        public DropTorrentDialogViewModel(ICacheService cacheService, string torrentPath)
        {
            _downloadService = new DownloadMediaService<MediaFile>(cacheService);
            CancellationDownloadingToken = new CancellationTokenSource();
            TorrentPath = torrentPath;
            CancelCommand = new RelayCommand(() =>
            {
                try
                {
                    CancellationDownloadingToken.Cancel();
                    CancellationDownloadingToken.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });

            Messenger.Default.Register<StopPlayMediaMessage>(this, e =>
            {
                CancelCommand.Execute(null);
            });
        }

        public async Task Download(int uploadLimit, int downloadLimit, Action buffered, Action cancelled)
        {
            var torrentType = TorrentPath.Contains("magnet:?") ? TorrentType.Magnet : TorrentType.File;

            var media = new MediaFile();
            var downloadProgress = new Progress<double>(e =>
            {
                DownloadProgress = e;
            });

            var downloadRateProgress = new Progress<BandwidthRate>(e =>
            {
                DownloadRate = e.DownloadRate;
            });

            var nbSeedsProgress = new Progress<int>(e =>
            {
                NbSeeders = e;
            });

            var nbPeersProgress = new Progress<int>(e =>
            {
                NbPeers = e;
            });

            await _downloadService.Download(media, torrentType, MediaType.Unkown, TorrentPath, uploadLimit,
                downloadLimit, downloadProgress, downloadRateProgress, nbSeedsProgress, nbPeersProgress, buffered,
                cancelled, CancellationDownloadingToken);
        }
    }
}