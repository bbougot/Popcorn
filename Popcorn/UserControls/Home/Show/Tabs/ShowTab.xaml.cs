using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Popcorn.Helpers;
using Popcorn.ViewModels.Pages.Home.Show.Tabs;

namespace Popcorn.UserControls.Home.Show.Tabs
{
    /// <summary>
    /// Logique d'interaction pour ShowTab.xaml
    /// </summary>
    public partial class ShowTab
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ShowTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is ShowTabsViewModel vm)) return;
            if (vm is PopularShowTabViewModel || vm is GreatestShowTabViewModel || vm is RecentShowTabViewModel ||
                vm is UpdatedShowTabViewModel ||
                vm is FavoritesShowTabViewModel)
            {
                if (!vm.IsLoadingShows && vm.NeedSync)
                {
                    await vm.LoadShowsAsync(true);
                    vm.NeedSync = false;
                }
            }
            else if (vm is SearchShowTabViewModel searchVm)
            {
                if (!searchVm.IsLoadingShows && searchVm.NeedSync)
                {
                    await searchVm.LoadShowsAsync(true);
                    searchVm.NeedSync = false;
                }
            }
        }

        /// <summary>
        /// Load shows if control has reached bottom position
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
            if (!(DataContext is ShowTabsViewModel vm))
            {
                _semaphore.Release();
                return;
            }

            switch (vm)
            {
                case PopularShowTabViewModel _:
                case GreatestShowTabViewModel _:
                case RecentShowTabViewModel _:
                case UpdatedShowTabViewModel _:
                case FavoritesShowTabViewModel _:
                    if (!vm.IsLoadingShows)
                        await vm.LoadShowsAsync();
                    break;
                case SearchShowTabViewModel searchVm:
                    if (!searchVm.IsLoadingShows)
                        await searchVm.LoadShowsAsync();
                    break;
            }

            _semaphore.Release();
        }
    }
}