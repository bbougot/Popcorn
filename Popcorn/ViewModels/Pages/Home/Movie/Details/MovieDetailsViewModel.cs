using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Enums;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Cast;
using Popcorn.Models.Movie;
using Popcorn.Models.Torrent;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.Movies.Trailer;
using Popcorn.Services.Subtitles;
using Popcorn.ViewModels.Pages.Home.Movie.Download;
using Popcorn.Services.Cache;
using Popcorn.Services.Download;
using Popcorn.Services.User;
using Popcorn.Exceptions;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;

namespace Popcorn.ViewModels.Pages.Home.Movie.Details
{
    /// <summary>
    /// Manage the movie
    /// </summary>
    public class MovieDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The service used to interact with movies
        /// </summary>
        private readonly IMovieService _movieService;

        /// <summary>
        /// Manage the movie download
        /// </summary>
        private DownloadMovieViewModel _downloadMovie;

        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        private bool _isDownloadingMovie;

        /// <summary>
        /// Specify if a movie is loading
        /// </summary>
        private bool _isMovieLoading;

        /// <summary>
        /// Specify if a trailer is playing
        /// </summary>
        private bool _isPlayingTrailer;

        /// <summary>
        /// Specify if a trailer is loading
        /// </summary>
        private bool _isTrailerLoading;

        /// <summary>
        /// True if similar movies are loading
        /// </summary>
        private bool _loadingSimilar;

        /// <summary>
        /// True if movie have similars
        /// </summary>
        private bool _anySimilar;

        /// <summary>
        /// Command used to set a movie as favorite
        /// </summary>
        public ICommand SetFavoriteMovieCommand { get; private set; }

        /// <summary>
        /// Command used to set a movie as watched
        /// </summary>
        public ICommand SetWatchedMovieCommand { get; private set; }

        /// <summary>
        /// Command used to stop loading a movie
        /// </summary>
        public ICommand StopLoadingMovieCommand { get; private set; }

        /// <summary>
        /// The movie to manage
        /// </summary>
        private MovieJson _movie = new MovieJson();

        /// <summary>
        /// The similar movies
        /// </summary>
        private ObservableCollection<MovieLightJson> _similarMovies;

        /// <summary>
        /// The movie trailer service
        /// </summary>
        private readonly IMovieTrailerService _movieTrailerService;

        /// <summary>
        /// The user service
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Token to cancel movie loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }

        /// <summary>
        /// Token to cancel trailer loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingTrailerToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the MovieDetailsViewModel class.
        /// </summary>
        /// <param name="movieService">Service used to interact with movies</param>
        /// <param name="movieTrailerService">The movie trailer service</param>
        /// <param name="subtitlesService">The subtitles service</param>
        /// <param name="cacheService">The cache service</param>
        /// <param name="userService">The user service</param>
        public MovieDetailsViewModel(IMovieService movieService, IMovieTrailerService movieTrailerService,
            ISubtitlesService subtitlesService, ICacheService cacheService, IUserService userService)
        {
            _movieTrailerService = movieTrailerService;
            _userService = userService;
            _movieService = movieService;
            Movie = new MovieJson();
            SimilarMovies = new ObservableCollection<MovieLightJson>();
            CancellationLoadingToken = new CancellationTokenSource();
            CancellationLoadingTrailerToken = new CancellationTokenSource();
            DownloadMovie = new DownloadMovieViewModel(subtitlesService,
                new DownloadMovieService<MovieJson>(cacheService), cacheService);
            RegisterMessages();
            RegisterCommands();
        }

        /// <summary>
        /// The selected movie to manage
        /// </summary>
        public MovieJson Movie
        {
            get => _movie;
            set { Set(() => Movie, ref _movie, value); }
        }

        /// <summary>
        /// The similar movies
        /// </summary>
        public ObservableCollection<MovieLightJson> SimilarMovies
        {
            get => _similarMovies;
            set { Set(() => SimilarMovies, ref _similarMovies, value); }
        }

        /// <summary>
        /// Indicates if a movie is loading
        /// </summary>
        public bool IsMovieLoading
        {
            get => _isMovieLoading;
            set { Set(() => IsMovieLoading, ref _isMovieLoading, value); }
        }

        /// <summary>
        /// Manage the movie download
        /// </summary>
        public DownloadMovieViewModel DownloadMovie
        {
            get => _downloadMovie;
            set { Set(() => DownloadMovie, ref _downloadMovie, value); }
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
        /// True if similar movies are loading
        /// </summary>
        public bool LoadingSimilar
        {
            get => _loadingSimilar;
            set { Set(() => LoadingSimilar, ref _loadingSimilar, value); }
        }

        /// <summary>
        /// True if movie have similars
        /// </summary>
        public bool AnySimilar
        {
            get => _anySimilar;
            set { Set(() => AnySimilar, ref _anySimilar, value); }
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
        /// Command used to load the movie
        /// </summary>
        public ICommand LoadMovieCommand { get; private set; }

        /// <summary>
        /// Command used to stop loading the trailer
        /// </summary>
        public ICommand StopLoadingTrailerCommand { get; private set; }

        /// <summary>
        /// Command used to play the movie
        /// </summary>
        public ICommand PlayMovieCommand { get; private set; }

        /// <summary>
        /// Command used to browse Imdb
        /// </summary>
        public ICommand GoToTmdbCommand { get; private set; }

        /// <summary>
        /// Command used to play the trailer
        /// </summary>
        public ICommand PlayTrailerCommand { get; private set; }

        /// <summary>
        /// Command used to search cast
        /// </summary>
        public ICommand SearchCastCommand { get; private set; }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Cleanup()
        {
            StopLoadingMovie();
            StopPlayingMovie();
            StopLoadingTrailer();
            StopPlayingTrailer();
            base.Cleanup();
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<StopPlayingTrailerMessage>(
                this,
                message =>
                {
                    if (message.Type == MediaType.Movie)
                    {
                        StopPlayingTrailer();
                    }
                });

            Messenger.Default.Register<StopPlayingMovieMessage>(
                this,
                message => { StopPlayingMovie(); });

            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                async message =>
                {
                    if (!string.IsNullOrEmpty(Movie?.ImdbId))
                    {
                        await _movieService.TranslateMovie(Movie);
                        foreach (var similar in SimilarMovies)
                        {
                            await _movieService.TranslateMovie(similar);
                        }
                    }
                });
        }

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            LoadMovieCommand = new RelayCommand<IMovie>(async movie =>
                await LoadMovie(movie, CancellationLoadingToken.Token));
            GoToTmdbCommand = new RelayCommand<string>(e =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = $"https://www.themoviedb.org/movie/{e}",
                    UseShellExecute = true
                };
                Process.Start(psi);
            });

            PlayMovieCommand = new RelayCommand(async () =>
            {
                Movie.AvailableTorrents = new ObservableCollection<ITorrent>(Movie.Torrents.Where(torrent => torrent.Peers.HasValue));
                var message = new ShowDownloadSettingsDialogMessage(Movie);
                await Messenger.Default.SendAsync(message);
                if (message.Download)
                {
                    IsDownloadingMovie = true;
                    Messenger.Default.Send(new DownloadMovieMessage(Movie));
                }
            });

            PlayTrailerCommand = new RelayCommand(async () =>
            {
                IsPlayingTrailer = true;
                IsTrailerLoading = true;
                await _movieTrailerService.LoadTrailerAsync(Movie, CancellationLoadingTrailerToken.Token);
                IsTrailerLoading = false;
            });

            StopLoadingTrailerCommand = new RelayCommand(StopLoadingTrailer);
            SearchCastCommand = new RelayCommand<CastJson>(cast =>
            {
                Messenger.Default.Send(new SearchCastMessage(cast));
            });

            SetFavoriteMovieCommand =
                new RelayCommand<bool>(isFavorite =>
                {
                    Movie.IsFavorite = isFavorite;
                    _userService.SetMovie(Movie);
                    Messenger.Default.Send(new ChangeFavoriteMovieMessage());
                });

            SetWatchedMovieCommand = new RelayCommand<bool>(hasBeenSeen =>
            {
                Movie.HasBeenSeen = hasBeenSeen;
                _userService.SetMovie(Movie);
                Messenger.Default.Send(new ChangeSeenMovieMessage());
            });

            StopLoadingMovieCommand = new RelayCommand(() =>
            {
                StopLoadingMovie();
                Messenger.Default.Send(new StopPlayingMovieMessage());
            });
        }

        /// <summary>
        /// Load the requested movie
        /// </summary>
        /// <param name="movie">The movie to load</param>
        /// <param name="ct">Cancellation</param>
        private async Task LoadMovie(IMovie movie, CancellationToken ct)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                Messenger.Default.Send(new LoadMovieMessage());
                Movie = new MovieJson {Title = movie.Title};
                IsMovieLoading = true;
                SimilarMovies.Clear();
                var applicationSettings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                Movie = await _movieService.GetMovieAsync(movie.ImdbId, ct);
                ct.ThrowIfCancellationRequested();
                await _movieService.TranslateMovie(Movie);
                _userService.SyncMovieHistory(new List<IMovie> {Movie});
                IsMovieLoading = false;
                Movie.FullHdAvailable = Movie.Torrents.Count > 1;
                Movie.WatchInFullHdQuality =
                    Movie.Torrents.Any(torrent => torrent.Quality == "1080p") && Movie.Torrents.Count == 1 ||
                    Movie.Torrents.Any(torrent => torrent.Quality == "1080p") &&
                    applicationSettings.DefaultHdQuality;
                LoadingSimilar = true;
                var similars = await _movieService.GetMoviesSimilarAsync(Movie, ct);
                SimilarMovies.AddRange(similars);
                AnySimilar = SimilarMovies.Any();
                LoadingSimilar = false;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed loading movie : {movie.ImdbId}. {ex.Message}");
                Messenger.Default.Send(new NavigateToHomePageMessage());
                if (!ct.IsCancellationRequested)
                    Messenger.Default.Send(new ManageExceptionMessage(new PopcornException(
                        $"{LocalizationProviderHelper.GetLocalizedValue<string>("FailedLoadingLabel")} {movie.Title}")));
            }
            finally
            {
                IsMovieLoading = false;
                LoadingSimilar = false;
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Logger.Trace($"LoadMovie ({movie.ImdbId}) in {elapsedMs} milliseconds.");
        }

        /// <summary>
        /// Stop loading the movie
        /// </summary>
        private void StopLoadingMovie()
        {
            if (IsMovieLoading)
            {
                Logger.Info(
                    $"Stop loading movie: {Movie.Title}.");

                IsMovieLoading = false;
                CancellationLoadingToken.Cancel();
                CancellationLoadingToken.Dispose();
                CancellationLoadingToken = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Stop playing the movie's trailer
        /// </summary>
        private void StopLoadingTrailer()
        {
            if (IsTrailerLoading)
            {
                Logger.Info(
                    $"Stop loading movie's trailer: {Movie.Title}.");

                IsTrailerLoading = false;
                CancellationLoadingTrailerToken.Cancel();
                CancellationLoadingTrailerToken.Dispose();
                CancellationLoadingTrailerToken = new CancellationTokenSource();
                StopPlayingTrailer();
            }
        }

        /// <summary>
        /// Stop playing the movie's trailer
        /// </summary>
        private void StopPlayingTrailer()
        {
            if (IsPlayingTrailer)
            {
                Logger.Info(
                    $"Stop playing movie's trailer: {Movie.Title}.");

                IsPlayingTrailer = false;
            }
        }

        /// <summary>
        /// Stop playing a movie
        /// </summary>
        private void StopPlayingMovie()
        {
            if (IsDownloadingMovie)
            {
                Logger.Info(
                    $"Stop playing movie: {Movie.Title}.");

                IsDownloadingMovie = false;
                DownloadMovie.StopDownloadingMovie();
            }
        }
    }
}