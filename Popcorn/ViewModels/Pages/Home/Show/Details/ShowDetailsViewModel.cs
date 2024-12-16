using System;
using System.Diagnostics;
using System.Net;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Messaging;
using Popcorn.Models.Shows;
using Popcorn.ViewModels.Pages.Home.Show.Download;
using Popcorn.Services.Subtitles;
using Popcorn.Services.Download;
using Popcorn.Models.Episode;
using Popcorn.Services.Shows.Trailer;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Popcorn.Enums;
using Popcorn.Helpers;
using Popcorn.Services.Cache;
using Popcorn.Services.Shows.Show;
using Popcorn.Exceptions;

namespace Popcorn.ViewModels.Pages.Home.Show.Details
{
    public class ShowDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The show trailer service
        /// </summary>
        private readonly IShowTrailerService _showTrailerService;

        /// <summary>
        /// The show service
        /// </summary>
        private readonly IShowService _showService;

        /// <summary>
        /// Command used to stop loading a show
        /// </summary>
        public ICommand StopLoadingShowCommand { get; private set; }

        /// <summary>
        /// The show
        /// </summary>
        private ShowJson _show;

        /// <summary>
        /// Specify if a trailer is playing
        /// </summary>
        private bool _isPlayingTrailer;

        /// <summary>
        /// Specify if a movie is loading
        /// </summary>
        private bool _isShowLoading;

        /// <summary>
        /// Specify if a trailer is loading
        /// </summary>
        private bool _isTrailerLoading;

        /// <summary>
        /// The download show view model instance
        /// </summary>
        private DownloadShowViewModel _downloadShowViewModel;

        /// <summary>
        /// Token to cancel trailer loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingTrailerToken { get; set; }

        /// <summary>
        /// Token to cancel show loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="showService">The show service</param>
        /// <param name="subtitlesService">The subtitles service</param>
        /// <param name="showTrailerService">The show trailer service</param>
        /// <param name="cacheService">The cache service</param>
        public ShowDetailsViewModel(IShowService showService, ISubtitlesService subtitlesService,
            IShowTrailerService showTrailerService, ICacheService cacheService)
        {
            _showTrailerService = showTrailerService;
            _showService = showService;
            Show = new ShowJson();
            RegisterCommands();
            RegisterMessages();
            CancellationLoadingTrailerToken = new CancellationTokenSource();
            CancellationLoadingToken = new CancellationTokenSource();
            var downloadService = new DownloadShowService<EpisodeShowJson>(cacheService);
            DownloadShow = new DownloadShowViewModel(downloadService, subtitlesService, cacheService);
        }

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            LoadShowCommand = new RelayCommand<ShowLightJson>(async show =>
                await LoadShow(show, CancellationLoadingToken.Token));
            GoToTMDbCommand = new RelayCommand<string>(e =>
            {
                Process.Start($"https://www.themoviedb.org/tv/{e}");
            });

            PlayTrailerCommand = new RelayCommand(async () =>
            {
                IsPlayingTrailer = true;
                IsTrailerLoading = true;
                await _showTrailerService.LoadTrailerAsync(Show, CancellationLoadingTrailerToken.Token);
                IsTrailerLoading = false;
            });

            StopLoadingTrailerCommand = new RelayCommand(StopLoadingTrailer);

            StopLoadingShowCommand = new RelayCommand(() =>
            {
                StopLoadingShow();
                Messenger.Default.Send(new StopPlayingEpisodeMessage());
            });
        }

        /// <summary>
        /// Indicates if a show is loading
        /// </summary>
        public bool IsShowLoading
        {
            get => _isShowLoading;
            set { Set(() => IsShowLoading, ref _isShowLoading, value); }
        }

        /// <summary>
        /// Specify if a trailer is loading
        /// </summary>
        public bool IsTrailerLoading
        {
            get => _isTrailerLoading;
            set { Set(() => IsTrailerLoading, ref _isTrailerLoading, value); }
        }

        /// <summary>
        /// Specify if a trailer is playing
        /// </summary>
        public bool IsPlayingTrailer
        {
            get => _isPlayingTrailer;
            set { Set(() => IsPlayingTrailer, ref _isPlayingTrailer, value); }
        }

        /// <summary>
        /// Command used to load the show's trailer
        /// </summary>
        public RelayCommand PlayTrailerCommand { get; private set; }

        /// <summary>
        /// Command used to browse Imdb
        /// </summary>
        public RelayCommand<string> GoToTMDbCommand { get; private set; }

        /// <summary>
        /// Command used to load the show
        /// </summary>
        public RelayCommand<ShowLightJson> LoadShowCommand { get; private set; }

        /// <summary>
        /// Command used to stop loading the trailer
        /// </summary>
        public RelayCommand StopLoadingTrailerCommand { get; private set; }

        /// <summary>
        /// The show
        /// </summary>
        public ShowJson Show
        {
            get => _show;
            set => Set(ref _show, value);
        }

        /// <summary>
        /// The download show view model instance
        /// </summary>
        public DownloadShowViewModel DownloadShow
        {
            get => _downloadShowViewModel;
            set => Set(ref _downloadShowViewModel, value);
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<StopPlayingEpisodeMessage>(
                this,
                message =>
                {
                    DownloadShow.StopDownloadingEpisode();
                });

            Messenger.Default.Register<StopPlayingTrailerMessage>(
                this,
                message =>
                {
                    if (message.Type == MediaType.Show)
                    {
                        StopPlayingTrailer();
                    }
                });
        }

        /// <summary>
        /// Load the requested show
        /// </summary>
        /// <param name="show">The show to load</param>
        /// <param name="ct">Cancellation token</param>
        private async Task LoadShow(ShowLightJson show, CancellationToken ct)
        {
            var watch = Stopwatch.StartNew();

            Messenger.Default.Send(new LoadShowMessage());
            Show = new ShowJson {Title = show.Title};
            IsShowLoading = true;
            try
            {
                Show = await _showService.GetShowAsync(show.ImdbId, ct);
                ct.ThrowIfCancellationRequested();
                foreach (var episode in Show.Episodes)
                {
                    episode.Title = WebUtility.HtmlDecode(episode.Title);
                    episode.ImdbId = Show.ImdbId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed loading show : {show.ImdbId}. {ex.Message}");
                Messenger.Default.Send(new NavigateToHomePageMessage());
                if (!ct.IsCancellationRequested)
                    Messenger.Default.Send(new ManageExceptionMessage(new PopcornException(
                        $"{LocalizationProviderHelper.GetLocalizedValue<string>("FailedLoadingLabel")} {show.Title}")));
            }
            finally
            {
                IsShowLoading = false;
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Logger.Trace($"LoadShow ({show.ImdbId}) in {elapsedMs} milliseconds.");
        }

        /// <summary>
        /// Stop playing the show's trailer
        /// </summary>
        private void StopLoadingTrailer()
        {
            if (IsTrailerLoading)
            {
                Logger.Info(
                    $"Stop loading show's trailer: {Show.Title}.");

                IsTrailerLoading = false;
                CancellationLoadingTrailerToken.Cancel();
                CancellationLoadingTrailerToken.Dispose();
                CancellationLoadingTrailerToken = new CancellationTokenSource();
                StopPlayingTrailer();
            }
        }

        /// <summary>
        /// Stop loading the movie
        /// </summary>
        private void StopLoadingShow()
        {
            if (IsShowLoading)
            {
                Logger.Info(
                    $"Stop loading show: {Show.Title}.");

                IsShowLoading = false;
                CancellationLoadingToken.Cancel();
                CancellationLoadingToken.Dispose();
                CancellationLoadingToken = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Stop playing the show's trailer
        /// </summary>
        private void StopPlayingTrailer()
        {
            if (IsPlayingTrailer)
            {
                Logger.Info(
                    $"Stop playing show's trailer: {Show.Title}.");

                IsPlayingTrailer = false;
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Cleanup()
        {
            StopLoadingTrailer();
            StopPlayingTrailer();
            base.Cleanup();
        }
    }
}