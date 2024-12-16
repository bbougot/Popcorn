using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NLog;
using Popcorn.Comparers;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Genres;
using Popcorn.Models.Movie;
using Popcorn.Services.Application;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.User;

namespace Popcorn.ViewModels.Pages.Home.Movie.Tabs
{
    /// <summary>
    /// Manage tab controls
    /// </summary>
    public class MovieTabsViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Semaphore loading
        /// </summary>
        protected readonly SemaphoreSlim LoadingSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Need sync movies
        /// </summary>
        public bool NeedSync;

        /// <summary>
        /// Vertical scroll offset
        /// </summary>
        private double _verticalScroll;

        /// <summary>
        /// The genre used to filter movies
        /// </summary>
        private static GenreJson _genre;

        /// <summary>
        /// The rating used to filter movies
        /// </summary>
        private static double _rating;

        /// <summary>
        /// Services used to interact with movie history
        /// </summary>
        protected readonly IUserService UserService;

        /// <summary>
        /// Services used to interact with movies
        /// </summary>
        protected readonly IMovieService MovieService;

        /// <summary>
        /// The current number of movies of the tab
        /// </summary>
        private int _currentNumberOfMovies;

        /// <summary>
        /// Specify if a movie loading has failed
        /// </summary>
        private bool _hasLoadingFailed;

        /// <summary>
        /// Specify if movies are loading
        /// </summary>
        private bool _isLoadingMovies;

        /// <summary>
        /// Indicates if there's any movie found
        /// </summary>
        private bool _isMoviesFound = true;

        /// <summary>
        /// The maximum number of movies found
        /// </summary>
        private int _maxNumberOfMovies;

        /// <summary>
        /// The tab's movies
        /// </summary>
        private ObservableCollection<MovieLightJson> _movies = new ObservableCollection<MovieLightJson>();

        /// <summary>
        /// The tab's name
        /// </summary>
        private string _tabName;

        /// <summary>
        /// Func which generates the tab name
        /// </summary>
        private readonly Func<string> _tabNameGenerator;

        /// <summary>
        /// Initializes a new instance of the MovieTabsViewModel class.
        /// </summary>
        /// <param name="applicationService">The application state</param>
        /// <param name="movieService">Used to interact with movies</param>
        /// <param name="userService">Used to interact with movie history</param>
        /// <param name="tabNameGenerator">Func which generates the tab name</param>
        protected MovieTabsViewModel(IApplicationService applicationService, IMovieService movieService,
            IUserService userService, Func<string> tabNameGenerator)
        {
            ApplicationService = applicationService;
            MovieService = movieService;
            UserService = userService;

            RegisterMessages();
            RegisterCommands();

            _tabNameGenerator = tabNameGenerator;
            TabName = tabNameGenerator.Invoke();
            MaxMoviesPerPage = Helpers.Constants.MaxMoviesPerPage;
            CancellationLoadingMovies = new CancellationTokenSource();
        }

        /// <summary>
        /// Application state
        /// </summary>
        public IApplicationService ApplicationService { get; }

        /// <summary>
        /// Tab's movies
        /// </summary>
        public ObservableCollection<MovieLightJson> Movies
        {
            get => _movies;
            set { Set(() => Movies, ref _movies, value); }
        }

        /// <summary>
        /// The current number of movies in the tab
        /// </summary>
        public int CurrentNumberOfMovies
        {
            get => _currentNumberOfMovies;
            set { Set(() => CurrentNumberOfMovies, ref _currentNumberOfMovies, value); }
        }

        /// <summary>
        /// The maximum number of movies found
        /// </summary>
        public int MaxNumberOfMovies
        {
            get => _maxNumberOfMovies;
            set { Set(() => MaxNumberOfMovies, ref _maxNumberOfMovies, value); }
        }

        /// <summary>
        /// The tab's name
        /// </summary>
        public string TabName
        {
            get => _tabName;
            set { Set(() => TabName, ref _tabName, value); }
        }

        /// <summary>
        /// Specify if movies are loading
        /// </summary>
        public bool IsLoadingMovies
        {
            get => _isLoadingMovies;
            protected set { Set(() => IsLoadingMovies, ref _isLoadingMovies, value); }
        }

        /// <summary>
        /// Indicates if there's any movie found
        /// </summary>
        public bool IsMovieFound
        {
            get => _isMoviesFound;
            set { Set(() => IsMovieFound, ref _isMoviesFound, value); }
        }

        /// <summary>
        /// The rating used to filter movies
        /// </summary>
        public double Rating
        {
            get => _rating;
            set { Set(() => Rating, ref _rating, value, true); }
        }

        /// <summary>
        /// Command used to reload movies
        /// </summary>
        public ICommand ReloadMovies { get; set; }

        /// <summary>
        /// Command used to set a movie as favorite
        /// </summary>
        public ICommand SetFavoriteMovieCommand { get; private set; }

        /// <summary>
        /// Command used to set a movie as watched
        /// </summary>
        public ICommand SetWatchedMovieCommand { get; private set; }

        /// <summary>
        /// Command used to change movie's genres
        /// </summary>
        public ICommand ChangeMovieGenreCommand { get; set; }

        /// <summary>
        /// Specify if a movie loading has failed
        /// </summary>
        public bool HasLoadingFailed
        {
            get => _hasLoadingFailed;
            set { Set(() => HasLoadingFailed, ref _hasLoadingFailed, value); }
        }

        /// <summary>
        /// Vertical scroll offset
        /// </summary>
        public double VerticalScroll
        {
            get => _verticalScroll;
            set { Set(() => VerticalScroll, ref _verticalScroll, value); }
        }

        /// <summary>
        /// The genre used to filter movies
        /// </summary>
        protected GenreJson Genre
        {
            get => _genre;
            private set { Set(() => Genre, ref _genre, value, true); }
        }

        /// <summary>
        /// Current page number of loaded movies
        /// </summary>
        protected int Page { get; set; }

        /// <summary>
        /// Maximum movies number to load per page request
        /// </summary>
        protected int MaxMoviesPerPage { get; set; }

        /// <summary>
        /// Token to cancel movie loading
        /// </summary>
        protected CancellationTokenSource CancellationLoadingMovies { get; private set; }

        /// <summary>
        /// The selected tab
        /// </summary>
        public static MovieTabsViewModel SelectedTab { get; set; }

        /// <summary>
        /// Sort by
        /// </summary>
        protected string SortBy { get; set; }

        /// <summary>
        /// Load movies asynchronously
        /// </summary>
        public virtual async Task LoadMoviesAsync(bool reset = false)
        {
            await LoadingSemaphore.WaitAsync(CancellationLoadingMovies.Token);
            await Task.Run(async () =>
            {
                if (reset)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Movies.Clear();
                        Page = 0;
                        VerticalScroll = 0d;
                    });
                }

                var watch = Stopwatch.StartNew();
                Page++;
                if (Page > 1 && Movies.Count == MaxNumberOfMovies)
                {
                    Page--;
                    LoadingSemaphore.Release();
                    return;
                }

                StopLoadingMovies();
                Logger.Trace(
                    $"Loading page {Page}...");
                HasLoadingFailed = false;
                try
                {
                    IsLoadingMovies = true;
                    var getMoviesWatcher = new Stopwatch();
                    getMoviesWatcher.Start();
                    var result =
                        await MovieService.GetMoviesAsync(Page,
                            MaxMoviesPerPage,
                            Rating,
                            SortBy,
                            CancellationLoadingMovies.Token,
                            Genre);
                    getMoviesWatcher.Stop();
                    var getMoviesEllapsedTime = getMoviesWatcher.ElapsedMilliseconds;
                    if (reset && getMoviesEllapsedTime < 500)
                    {
                        // Wait for VerticalOffset to reach 0 (animation lasts 500ms)
                        await Task.Delay(500 - (int) getMoviesEllapsedTime, CancellationLoadingMovies.Token);
                    }

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Movies.AddRange(result.movies.Except(Movies, new MovieLightComparer()));
                        IsLoadingMovies = false;
                        IsMovieFound = Movies.Any();
                        CurrentNumberOfMovies = Movies.Count;
                        MaxNumberOfMovies = result.nbMovies == 0 ? Movies.Count : result.nbMovies;
                        UserService.SyncMovieHistory(Movies);
                    });
                }
                catch (Exception exception)
                {
                    Page--;
                    Logger.Error(
                        $"Error while loading page {Page}: {exception.Message}");
                    HasLoadingFailed = true;
                    Messenger.Default.Send(new ManageExceptionMessage(exception));
                }
                finally
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Logger.Trace(
                        $"Loaded page {Page} in {elapsedMs} milliseconds.");
                    LoadingSemaphore.Release();
                }
            });
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Cleanup()
        {
            StopLoadingMovies();
            base.Cleanup();
        }

        /// <summary>
        /// Cancel the loading of the next page
        /// </summary>
        protected void StopLoadingMovies()
        {
            CancellationLoadingMovies.Cancel();
            CancellationLoadingMovies.Dispose();
            CancellationLoadingMovies = new CancellationTokenSource();
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                async message =>
                {
                    var movies = Movies.ToList();
                    foreach (var movie in movies)
                    {
                        await MovieService.TranslateMovie(movie);
                    }
                });

            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                language => TabName = _tabNameGenerator.Invoke());

            Messenger.Default.Register<PropertyChangedMessage<GenreJson>>(this, async e =>
            {
                if (e.PropertyName != GetPropertyName(() => Genre) ||
                    e.PropertyName != GetPropertyName(() => Genre) && Genre.Equals(e.NewValue) ||
                    !(e.Sender is MovieTabsViewModel)) return;
                _genre = e.NewValue;
                if (SelectedTab == this)
                {
                    await LoadMoviesAsync(true);
                }
                else
                {
                    NeedSync = true;
                }
            });

            Messenger.Default.Register<PropertyChangedMessage<double>>(this, async e =>
            {
                if (e.PropertyName != GetPropertyName(() => Rating) ||
                    e.PropertyName != GetPropertyName(() => Rating) && Rating.Equals(e.NewValue) ||
                    !(e.Sender is MovieTabsViewModel)) return;
                _rating = e.NewValue;
                if (SelectedTab == this)
                {
                    await LoadMoviesAsync(true);
                }
                else
                {
                    NeedSync = true;
                }
            });

            Messenger.Default.Register<ChangeFavoriteMovieMessage>(
                this,
                async message =>
                {
                    UserService.SyncMovieHistory(Movies);
                    if (this is FavoritesMovieTabViewModel && !(SelectedTab is FavoritesMovieTabViewModel) ||
                        this is RecommendationsMovieTabViewModel && !(SelectedTab is RecommendationsMovieTabViewModel))
                        NeedSync = true;
                    else if (this is FavoritesMovieTabViewModel && SelectedTab is FavoritesMovieTabViewModel ||
                             this is RecommendationsMovieTabViewModel &&
                             SelectedTab is RecommendationsMovieTabViewModel)
                    {
                        NeedSync = true;
                        await LoadMoviesAsync(this is RecommendationsMovieTabViewModel);
                    }
                });

            Messenger.Default.Register<ChangeSeenMovieMessage>(
                this,
                async message =>
                {
                    UserService.SyncMovieHistory(Movies);
                    if (this is SeenMovieTabViewModel && !(SelectedTab is SeenMovieTabViewModel) ||
                        this is RecommendationsMovieTabViewModel && !(SelectedTab is RecommendationsMovieTabViewModel))
                        NeedSync = true;
                    else if (this is SeenMovieTabViewModel && SelectedTab is SeenMovieTabViewModel ||
                             this is RecommendationsMovieTabViewModel &&
                             SelectedTab is RecommendationsMovieTabViewModel)
                    {
                        NeedSync = true;
                        await LoadMoviesAsync(this is RecommendationsMovieTabViewModel);
                    }
                });
        }

        /// <summary>
        /// Register commands
        /// </summary>
        /// <returns></returns>
        private void RegisterCommands()
        {
            ReloadMovies = new RelayCommand(async () =>
            {
                ApplicationService.IsConnectionInError = false;
                await LoadMoviesAsync();
            });

            SetFavoriteMovieCommand =
                new RelayCommand<MovieLightJson>(movie =>
                {
                    UserService.SetMovie(movie);
                    Messenger.Default.Send(new ChangeFavoriteMovieMessage());
                });

            SetWatchedMovieCommand = new RelayCommand<MovieLightJson>(movie =>
            {
                UserService.SetMovie(movie);
                Messenger.Default.Send(new ChangeSeenMovieMessage());
            });

            ChangeMovieGenreCommand =
                new RelayCommand<GenreJson>(genre =>
                {
                    if (genre.Name == LocalizationProviderHelper.GetLocalizedValue<string>(
                            "AllLabel"))
                    {
                        Genre = null;
                    }
                    else if (genre.Name == "Science-Fiction")
                    {
                        genre.EnglishName = "Sci-Fi";
                        Genre = genre;
                    }
                    else
                    {
                        Genre = genre;
                    }
                });
        }
    }
}