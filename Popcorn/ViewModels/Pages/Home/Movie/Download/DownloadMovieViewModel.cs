using System;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Movie;
using Popcorn.Services.Subtitles;
using Popcorn.Utils;
using Popcorn.Services.Download;
using GalaSoft.MvvmLight.Ioc;
using Popcorn.Enums;
using Popcorn.Models.Bandwidth;
using Popcorn.Services.Cache;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;

namespace Popcorn.ViewModels.Pages.Home.Movie.Download
{
    /// <summary>
    /// Manage the download of a movie
    /// </summary>
    public class DownloadMovieViewModel : ViewModelBase
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
        private readonly IDownloadService<MovieJson> _downloadService;

        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        private bool _isDownloadingMovie;

        /// <summary>
        /// The movie to download
        /// </summary>
        private MovieJson _movie;

        /// <summary>
        /// The movie download progress
        /// </summary>
        private double _movieDownloadProgress;

        /// <summary>
        /// The movie download rate
        /// </summary>
        private double _movieDownloadRate;

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
        private CancellationTokenSource CancellationDownloadingMovie { get; set; }

        /// <summary>
        /// The cache service
        /// </summary>
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Initializes a new instance of the DownloadMovieViewModel class.
        /// </summary>
        /// <param name="subtitlesService">Instance of SubtitlesService</param>
        /// <param name="downloadService">Download service</param>
        /// <param name="cacheService">Cache service</param>
        public DownloadMovieViewModel(ISubtitlesService subtitlesService, IDownloadService<MovieJson> downloadService,
            ICacheService cacheService)
        {
            _cacheService = cacheService;
            _subtitlesService = subtitlesService;
            _downloadService = downloadService;
            CancellationDownloadingMovie = new CancellationTokenSource();
            RegisterMessages();
            RegisterCommands();
        }

        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        public bool IsDownloadingMovie
        {
            get => _isDownloadingMovie;
            set { Set(() => IsDownloadingMovie, ref _isDownloadingMovie, value); }
        }

        /// <summary>
        /// Specify the movie download progress
        /// </summary>
        public double MovieDownloadProgress
        {
            get => _movieDownloadProgress;
            set { Set(() => MovieDownloadProgress, ref _movieDownloadProgress, value); }
        }

        /// <summary>
        /// Specify the movie download rate
        /// </summary>
        public double MovieDownloadRate
        {
            get => _movieDownloadRate;
            set { Set(() => MovieDownloadRate, ref _movieDownloadRate, value); }
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
        /// The movie to download
        /// </summary>
        public MovieJson Movie
        {
            get => _movie;
            set { Set(() => Movie, ref _movie, value); }
        }

        /// <summary>
        /// The command used to stop the download of a movie
        /// </summary>
        public RelayCommand StopDownloadingMovieCommand { get; private set; }

        /// <summary>
        /// Stop downloading a movie
        /// </summary>
        public void StopDownloadingMovie()
        {
            Logger.Info(
                $"Stop downloading the movie {Movie.Title}.");

            IsDownloadingMovie = false;
            CancellationDownloadingMovie.Cancel();
            CancellationDownloadingMovie.Dispose();
            CancellationDownloadingMovie = new CancellationTokenSource();

            if (!string.IsNullOrEmpty(Movie?.FilePath))
            {
                Movie.FilePath = string.Empty;
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Cleanup()
        {
            StopDownloadingMovie();
            base.Cleanup();
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages() => Messenger.Default.Register<DownloadMovieMessage>(
            this,
            async message =>
            {
                if (IsDownloadingMovie)
                    return;

                IsDownloadingMovie = true;
                Movie = message.Movie;
                MovieDownloadRate = 0d;
                MovieDownloadProgress = 0d;
                NbPeers = 0;
                NbSeeders = 0;
                var reportDownloadProgress = new Progress<double>(ReportMovieDownloadProgress);
                var reportDownloadRate = new Progress<BandwidthRate>(ReportMovieDownloadRate);
                var reportNbPeers = new Progress<int>(ReportNbPeers);
                var reportNbSeeders = new Progress<int>(ReportNbSeeders);
                try
                {
                    if (message.Movie.SelectedSubtitle != null &&
                        message.Movie.SelectedSubtitle.LanguageName !=
                        LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"))
                    {
                        var path = Path.Combine(_cacheService.Subtitles + message.Movie.ImdbId);
                        Directory.CreateDirectory(path);
                        var isRemote = true;
                        if (message.Movie.SelectedSubtitle.LanguageName == LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"))
                        {
                            isRemote = false;
                            message.Movie.SelectedSubtitle.LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel");
                            message.Movie.SelectedSubtitle.SubDownloadLink = message.Movie.SelectedSubtitle.FilePath;
                        }

                        var subtitlePath = await
                            _subtitlesService.DownloadSubtitleToPath(path,
                                message.Movie.SelectedSubtitle, isRemote);
                        message.Movie.SelectedSubtitle.FilePath = subtitlePath;
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
                        var torrentUrl = Movie.WatchInFullHdQuality
                            ? Movie.Torrents?.FirstOrDefault(torrent => torrent.Quality == "1080p")?.Url
                            : Movie.Torrents?.FirstOrDefault(torrent => torrent.Quality == "720p")?.Url;

                        var result =
                            await
                                DownloadFileHelper.DownloadFileTaskAsync(torrentUrl,
                                    _cacheService.MovieTorrentDownloads + Movie.ImdbId + ".torrent");
                        var torrentPath = string.Empty;
                        if (result.Item3 == null && !string.IsNullOrEmpty(result.Item2))
                            torrentPath = result.Item2;

                        var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                        await _downloadService.Download(Movie, TorrentType.File, MediaType.Movie, torrentPath,
                                settings.UploadLimit, settings.DownloadLimit, reportDownloadProgress,
                                reportDownloadRate, reportNbSeeders, reportNbPeers, () => { }, () => { },
                                CancellationDownloadingMovie);
                    }
                    catch (Exception ex)
                    {
                        // An error occured.
                        Messenger.Default.Send(new ManageExceptionMessage(ex));
                        Messenger.Default.Send(new StopPlayingMovieMessage());
                    }
                }
            });

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands() => StopDownloadingMovieCommand = new RelayCommand(() =>
        {
            Messenger.Default.Send(new StopPlayingMovieMessage());
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
        private void ReportMovieDownloadRate(BandwidthRate value) => MovieDownloadRate = value.DownloadRate;

        /// <summary>
        /// Report the download progress
        /// </summary>
        /// <param name="value">The download progress to report</param>
        private void ReportMovieDownloadProgress(double value)
        {
            MovieDownloadProgress = value;
        }
    }
}