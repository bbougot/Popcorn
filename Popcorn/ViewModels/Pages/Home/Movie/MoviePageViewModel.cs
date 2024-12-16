using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Messaging;
using Popcorn.Models.Movie;
using Popcorn.Services.Application;
using Popcorn.Services.Genres;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.User;
using Popcorn.ViewModels.Pages.Home.Genres;
using Popcorn.ViewModels.Pages.Home.Movie.Search;
using Popcorn.ViewModels.Pages.Home.Movie.Tabs;

namespace Popcorn.ViewModels.Pages.Home.Movie
{
    public class MoviePageViewModel : ObservableObject, IPageViewModel
    {
        /// <summary>
        /// <see cref="Caption"/>
        /// </summary>
        private string _caption;

        /// <summary>
        /// Used to interact with movie history
        /// </summary>
        private IUserService UserService { get; }

        /// <summary>
        /// Used to interact with movies
        /// </summary>
        private IMovieService MovieService { get; }

        /// <summary>
        /// Application state
        /// </summary>
        private IApplicationService _applicationService;

        /// <summary>
        /// Manage genres
        /// </summary>
        private GenreViewModel _genreViewModel;

        /// <summary>
        /// Specify if a search is actually active
        /// </summary>
        private bool _isSearchActive;

        /// <summary>
        /// <see cref="SelectedMoviesIndexMenuTab"/>
        /// </summary>
        private int _selectedMoviesIndexMenuTab;

        /// <summary>
        /// The selected tab
        /// </summary>
        private MovieTabsViewModel _selectedTab;

        /// <summary>
        /// <see cref="Search"/>
        /// </summary>
        private SearchMovieViewModel _search;

        /// <summary>
        /// The tabs
        /// </summary>
        private ObservableCollection<MovieTabsViewModel> _tabs = new ObservableCollection<MovieTabsViewModel>();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="movieService">Instance of MovieService</param>
        /// <param name="userService">Instance of UserService</param>
        /// <param name="applicationService">Instance of ApplicationService</param>
        /// <param name="genreService">The genre service</param>
        public MoviePageViewModel(IMovieService movieService,
            IUserService userService, IApplicationService applicationService, IGenreService genreService)
        {
            MovieService = movieService;
            UserService = userService;
            ApplicationService = applicationService;
            GenreViewModel = new GenreViewModel(userService, genreService);
            RegisterMessages();
            RegisterCommands();

            Search = new SearchMovieViewModel();
            Tabs.Add(new PopularMovieTabViewModel(ApplicationService, MovieService, UserService));
            Tabs.Add(new GreatestMovieTabViewModel(ApplicationService, MovieService, UserService));
            Tabs.Add(new RecentMovieTabViewModel(ApplicationService, MovieService, UserService));
            Tabs.Add(new FavoritesMovieTabViewModel(ApplicationService, MovieService, UserService));
            Tabs.Add(new SeenMovieTabViewModel(ApplicationService, MovieService, UserService));
            Tabs.Add(new RecommendationsMovieTabViewModel(ApplicationService, MovieService, UserService));
            SelectedTab = Tabs.First();
            SelectedMoviesIndexMenuTab = 0;
        }

        /// <summary>
        /// Manage the movie search
        /// </summary>
        public SearchMovieViewModel Search
        {
            get => _search;
            set => Set(ref _search, value);
        }

        /// <summary>
        /// Tab caption 
        /// </summary>
        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }

        /// <summary>
        /// Specify if a movie search is active
        /// </summary>
        public bool IsSearchActive
        {
            get => _isSearchActive;
            private set { Set(() => IsSearchActive, ref _isSearchActive, value); }
        }

        /// <summary>
        /// Tabs shown into the interface
        /// </summary>
        public ObservableCollection<MovieTabsViewModel> Tabs
        {
            get => _tabs;
            set { Set(() => Tabs, ref _tabs, value); }
        }

        /// <summary>
        /// The selected tab
        /// </summary>
        public MovieTabsViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                Set(() => SelectedTab, ref _selectedTab, value);
                MovieTabsViewModel.SelectedTab = _selectedTab;
            }
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<SearchMovieMessage>(this,
                async message => await SearchMovies(message.Filter));
        }

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            SelectGreatestTab = new RelayCommand(() =>
            {
                if (SelectedTab is GreatestMovieTabViewModel)
                    return;
                foreach (var greatestTab in Tabs.OfType<GreatestMovieTabViewModel>().ToList())
                    SelectedTab = greatestTab;
            });

            SelectPopularTab = new RelayCommand(() =>
            {
                if (SelectedTab is PopularMovieTabViewModel)
                    return;
                foreach (var popularTab in Tabs.OfType<PopularMovieTabViewModel>().ToList())
                    SelectedTab = popularTab;
            });

            SelectRecentTab = new RelayCommand(() =>
            {
                if (SelectedTab is RecentMovieTabViewModel)
                    return;
                foreach (var recentTab in Tabs.OfType<RecentMovieTabViewModel>().ToList())
                    SelectedTab = recentTab;
            });

            SelectSearchTab = new RelayCommand(() =>
            {
                if (SelectedTab is SearchMovieTabViewModel)
                    return;
                foreach (var searchTab in Tabs.OfType<SearchMovieTabViewModel>().ToList())
                    SelectedTab = searchTab;
            });

            SelectFavoritesTab = new RelayCommand(() =>
            {
                if (SelectedTab is FavoritesMovieTabViewModel)
                    return;
                foreach (var favoritesTab in Tabs.OfType<FavoritesMovieTabViewModel>().ToList())
                    SelectedTab = favoritesTab;
            });

            SelectSeenTab = new RelayCommand(() =>
            {
                if (SelectedTab is SeenMovieTabViewModel)
                    return;
                foreach (var seenTab in Tabs.OfType<SeenMovieTabViewModel>().ToList())
                    SelectedTab = seenTab;
            });

            SelectRecommendationsTab = new RelayCommand(() =>
            {
                if (SelectedTab is RecommendationsMovieTabViewModel)
                    return;
                foreach (var recommendationTab in Tabs.OfType<RecommendationsMovieTabViewModel>().ToList())
                    SelectedTab = recommendationTab;
            });
        }

        /// <summary>
        /// Manage movie's genres
        /// </summary>
        public GenreViewModel GenreViewModel
        {
            get => _genreViewModel;
            set { Set(() => GenreViewModel, ref _genreViewModel, value); }
        }

        /// <summary>
        /// Application state
        /// </summary>
        public IApplicationService ApplicationService
        {
            get => _applicationService;
            set { Set(() => ApplicationService, ref _applicationService, value); }
        }

        /// <summary>
        /// Command used to select the greatest movies tab
        /// </summary>
        public ICommand SelectGreatestTab { get; private set; }

        /// <summary>
        /// Command used to select the popular movies tab
        /// </summary>
        public ICommand SelectPopularTab { get; private set; }

        /// <summary>
        /// Command used to select the recent movies tab
        /// </summary>
        public ICommand SelectRecentTab { get; private set; }

        /// <summary>
        /// Command used to select the search movies tab
        /// </summary>
        public ICommand SelectSearchTab { get; private set; }

        /// <summary>
        /// Command used to select the seen movies tab
        /// </summary>
        public ICommand SelectSeenTab { get; private set; }

        /// <summary>
        /// Command used to select the favorites movies tab
        /// </summary>
        public ICommand SelectFavoritesTab { get; private set; }

        /// <summary>
        /// Command used to select the recommendations movies tab
        /// </summary>
        public ICommand SelectRecommendationsTab { get; private set; }


        /// <summary>
        /// Selected index for movies menu
        /// </summary>
        public int SelectedMoviesIndexMenuTab
        {
            get => _selectedMoviesIndexMenuTab;
            set { Set(() => SelectedMoviesIndexMenuTab, ref _selectedMoviesIndexMenuTab, value); }
        }

        /// <summary>
        /// Search for movie with a criteria
        /// </summary>
        /// <param name="criteria">The criteria used for search</param>
        private async Task SearchMovies(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                // The search filter is empty. We have to find the search tab if any
                foreach (var searchTabToRemove in Tabs.OfType<SearchMovieTabViewModel>().ToList().ToList())
                {
                    // The search tab is currently selected in the UI, we have to pick a different selected tab prior deleting
                    if (searchTabToRemove == SelectedTab)
                        SelectedTab = Tabs.FirstOrDefault();

                    Tabs.Remove(searchTabToRemove);
                    searchTabToRemove.Cleanup();
                    IsSearchActive = false;
                    SelectedMoviesIndexMenuTab = 0;
                }
            }
            else
            {
                IsSearchActive = true;
                SelectedMoviesIndexMenuTab = 3;
                if (Tabs.OfType<SearchMovieTabViewModel>().Any())
                {
                    foreach (var searchTab in Tabs.OfType<SearchMovieTabViewModel>().ToList())
                    {
                        searchTab.SearchFilter = criteria;
                        await searchTab.LoadMoviesAsync(true);
                        if (SelectedTab != searchTab)
                            SelectedTab = searchTab;
                    }
                }
                else
                {
                    Tabs.Add(new SearchMovieTabViewModel(ApplicationService, MovieService, UserService));
                    SelectedTab = Tabs.Last();
                    var searchMovieTab = SelectedTab as SearchMovieTabViewModel;
                    if (searchMovieTab != null)
                    {
                        searchMovieTab.SearchFilter = criteria;
                        await searchMovieTab.LoadMoviesAsync(true);
                    }
                }
            }
        }
    }
}