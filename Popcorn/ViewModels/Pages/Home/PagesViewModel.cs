using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.ViewModels.Pages.Home.Movie;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Show;

namespace Popcorn.ViewModels.Pages.Home
{
    /// <summary>
    /// Represents a page
    /// </summary>
    public class PagesViewModel : ViewModelBase
    {
        /// <summary>
        /// The pages
        /// </summary>
        private ObservableCollection<IPageViewModel> _pages = new ObservableCollection<IPageViewModel>();

        /// <summary>
        /// Pages shown into the interface
        /// </summary>
        public ObservableCollection<IPageViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        /// <summary>
        /// Create an instance of <see cref="PagesViewModel"/>
        /// </summary>
        /// <param name="moviePage">Movie page</param>
        /// <param name="showPage">Show page</param>
        /// <param name="settingsPageViewModel">Settings page</param>
        public PagesViewModel(MoviePageViewModel moviePage, ShowPageViewModel showPage,
            SettingsPageViewModel settingsPageViewModel)
        {
            moviePage.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("MoviesLabel");
            showPage.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("ShowsLabel");
            settingsPageViewModel.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("SettingsLabel");
            Pages = new ObservableCollection<IPageViewModel>
            {
                moviePage,
                showPage,
                settingsPageViewModel
            };
            
            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                message =>
                {
                    foreach (var page in Pages)
                    {
                        if (page is MoviePageViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("MoviesLabel");
                        }
                        else if (page is ShowPageViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("ShowsLabel");
                        }
                        else if (page is SettingsPageViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("SettingsLabel");
                        }
                    }
                });
        }
    }
}