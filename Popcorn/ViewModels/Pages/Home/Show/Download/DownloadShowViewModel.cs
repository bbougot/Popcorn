using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Episode;
using Popcorn.Services.Subtitles;
using Popcorn.Utils;
using GalaSoft.MvvmLight.Ioc;
using Popcorn.Enums;
using Popcorn.Models.Bandwidth;
using Popcorn.Services.Cache;
using Popcorn.Services.Download;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;

namespace Popcorn.ViewModels.Pages.Home.Show.Download
{
    public class DownloadShowViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Used to interact with subtitles
        /// </summary>
        private readonly ISubtitlesService _subtitlesService;

        /// <summary>
        /// The download service
        /// </summary>
        private readonly IDownloadService<EpisodeShowJson> _downloadService;

        /// <summary>
        /// Specify if an episode is downloading
        /// </summary>
        private bool _isDownloadingEpisode;

        /// <summary>
        /// The episode to download
        /// </summary>
        private EpisodeShowJson _episode;

        /// <summary>
        /// The episode download progress
        /// </summary>
        private double _episodeDownloadProgress;

        /// <summary>
        /// The episode download rate
        /// </summary>
        private double _episodeDownloadRate;

        /// <summary>
        /// Number of seeders
        /// </summary>
        private int _nbSeeders;

        /// <summary>
        /// Number of peers
        /// </summary>
        private int _nbPeers;

        /// <summary>
        /// Token to cancel the download
        /// </summary>
        private CancellationTokenSource CancellationDownloadingEpisode { get; set; }

        /// <summary>
        /// The cache service
        /// </summary>
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="downloadService">The download service</param>
        /// <param name="subtitlesService">The subtitles service</param>
        /// <param name="cacheService">The cache service</param>
        public DownloadShowViewModel(IDownloadService<EpisodeShowJson> downloadService,
            ISubtitlesService subtitlesService, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _downloadService = downloadService;
            _subtitlesService = subtitlesService;
            CancellationDownloadingEpisode = new CancellationTokenSource();
            RegisterCommands();
            RegisterMessages();
        }

        /// <summary>
        /// Specify if an episode is downloading
        /// </summary>
        public bool IsDownloadingEpisode
        {
            get => _isDownloadingEpisode;
            set { Set(() => IsDownloadingEpisode, ref _isDownloadingEpisode, value); }
        }

        /// <summary>
        /// Specify the episode download progress
        /// </summary>
        public double EpisodeDownloadProgress
        {
            get => _episodeDownloadProgress;
            set { Set(() => EpisodeDownloadProgress, ref _episodeDownloadProgress, value); }
        }

        /// <summary>
        /// Specify the episode download rate
        /// </summary>
        public double EpisodeDownloadRate
        {
            get => _episodeDownloadRate;
            set { Set(() => EpisodeDownloadRate, ref _episodeDownloadRate, value); }
        }

        /// <summary>
        /// Number of peers
        /// </summary>
        public int NbPeers
        {
            get => _nbPeers;
            set { Set(() => NbPeers, ref _nbPeers, value); }
        }

        /// <summary>
        /// Number of seeders
        /// </summary>
        public int NbSeeders
        {
            get => _nbSeeders;
            set { Set(() => NbSeeders, ref _nbSeeders, value); }
        }

        /// <summary>
        /// The episode to download
        /// </summary>
        public EpisodeShowJson Episode
        {
            get => _episode;
            set { Set(() => Episode, ref _episode, value); }
        }

        /// <summary>
        /// The command used to stop the download of an episode
        /// </summary>
        public RelayCommand StopDownloadingEpisodeCommand { get; private set; }

        /// <summary>
        /// Stop downloading an episode
        /// </summary>
        public void StopDownloadingEpisode()
        {
            Logger.Info(
                $"Stop downloading the episode {Episode.Title}");

            IsDownloadingEpisode = false;
            CancellationDownloadingEpisode.Cancel();
            CancellationDownloadingEpisode.Dispose();
            CancellationDownloadingEpisode = new CancellationTokenSource();

            if (!string.IsNullOrEmpty(Episode?.FilePath))
            {
                Episode.FilePath = string.Empty;
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Cleanup()
        {
            StopDownloadingEpisode();
            base.Cleanup();
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages() => Messenger.Default.Register<DownloadShowEpisodeMessage>(
            this,
            async message =>
            {
                if (IsDownloadingEpisode)
                    return;

                IsDownloadingEpisode = true;
                Episode = message.Episode;
                EpisodeDownloadRate = 0d;
                EpisodeDownloadProgress = 0d;
                NbPeers = 0;
                NbSeeders = 0;
                var reportDownloadProgress = new Progress<double>(ReportEpisodeDownloadProgress);
                var reportDownloadRate = new Progress<BandwidthRate>(ReportEpisodeDownloadRate);
                var reportNbPeers = new Progress<int>(ReportNbPeers);
                var reportNbSeeders = new Progress<int>(ReportNbSeeders);

                try
                {
                    if (message.Episode.SelectedSubtitle != null &&
                        message.Episode.SelectedSubtitle.LanguageName !=
                        LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"))
                    {
                        var path = Path.Combine(_cacheService.Subtitles + message.Episode.ImdbId);
                        Directory.CreateDirectory(path);
                        var subtitlePath =
                            await _subtitlesService.DownloadSubtitleToPath(path,
                                message.Episode.SelectedSubtitle);

                        message.Episode.SelectedSubtitle.FilePath = subtitlePath;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
                finally
                {
                    try
                    {
                        var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                        await _downloadService.Download(message.Episode, TorrentType.Magnet, MediaType.Show,
                            Episode.SelectedTorrent.Url, settings.UploadLimit, settings.DownloadLimit,
                            reportDownloadProgress,
                            reportDownloadRate, reportNbSeeders, reportNbPeers, () => { }, () => { },
                            CancellationDownloadingEpisode);
                    }
                    catch (Exception ex)
                    {
                        // An error occured.
                        Messenger.Default.Send(new ManageExceptionMessage(ex));
                        Messenger.Default.Send(new StopPlayingEpisodeMessage());
                    }
                }
            });

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands() => StopDownloadingEpisodeCommand = new RelayCommand(() =>
        {
            StopDownloadingEpisode();
            Messenger.Default.Send(new StopPlayingEpisodeMessage());
        });

        /// <summary>
        /// Report the number of seeders
        /// </summary>
        /// <param name="value">Number of seeders</param>
        private void ReportNbSeeders(int value) => NbSeeders = value;

        /// <summary>
        /// Report the number of peers
        /// </summary>
        /// <param name="value">Nubmer of peers</param>
        private void ReportNbPeers(int value) => NbPeers = value;

        /// <summary>
        /// Report the download progress
        /// </summary>
        /// <param name="value">Download rate</param>
        private void ReportEpisodeDownloadRate(BandwidthRate value) => EpisodeDownloadRate = value.DownloadRate;

        /// <summary>
        /// Report the download progress
        /// </summary>
        /// <param name="value">The download progress to report</param>
        private void ReportEpisodeDownloadProgress(double value)
        {
            EpisodeDownloadProgress = value;
        }
    }
}