using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Popcorn.Helpers;
using Popcorn.ViewModels.Pages.Home.Movie.Tabs;

namespace Popcorn.UserControls.Home.Movie.Tabs
{
    /// <summary>
    /// Interaction logic for MovieTab.xaml
    /// </summary>
    public partial class MovieTab
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the MovieTab class.
        /// </summary>
        public MovieTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is MovieTabsViewModel vm)) return;
            if (vm is PopularMovieTabViewModel || vm is GreatestMovieTabViewModel || vm is RecentMovieTabViewModel ||
                vm is FavoritesMovieTabViewModel || vm is SeenMovieTabViewModel ||
                vm is RecommendationsMovieTabViewModel)
            {
                if (!vm.IsLoadingMovies && vm.NeedSync)
                {
                    await vm.LoadMoviesAsync(true);
                    vm.NeedSync = false;
                }
            }
            else if (vm is SearchMovieTabViewModel searchVm && !searchVm.IsLoadingMovies && searchVm.NeedSync)
            {
                await searchVm.LoadMoviesAsync(true);
                searchVm.NeedSync = false;
            }
        }

        /// <summary>
        /// Load movies if control has reached bottom position
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private async void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var totalHeight = e.VerticalOffset + e.ViewportHeight;
            if (e.VerticalChange < 0 || totalHeight < 2d / 3d * e.ExtentHeight)
            {
                return;
            }

            if (_semaphore.CurrentCount == 0)
            {
                return;
            }

            await _semaphore.WaitAsync();
            if (!(DataContext is MovieTabsViewModel vm))
            {
                _semaphore.Release();
                return;
            }

            switch (vm)
            {
                case PopularMovieTabViewModel _:
                case GreatestMovieTabViewModel _:
                case RecentMovieTabViewModel _:
                case FavoritesMovieTabViewModel _:
                case SeenMovieTabViewModel _:
                case RecommendationsMovieTabViewModel _:
                    if (!vm.IsLoadingMovies)
                        await vm.LoadMoviesAsync();
                    break;
                case SearchMovieTabViewModel searchVm:
                    if (!searchVm.IsLoadingMovies)
                        await searchVm.LoadMoviesAsync();
                    break;
            }

            _semaphore.Release();
        }
    }
}