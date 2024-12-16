using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.ViewModels.Pages.Home.Settings.About;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;
using Popcorn.ViewModels.Pages.Home.Settings.Help;

namespace Popcorn.ViewModels.Pages.Home.Settings
{
    public class SettingsPageViewModel : ViewModelBase, IPageViewModel
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// <see cref="Caption"/>
        /// </summary>
        private string _caption;

        /// <summary>
        /// The pages
        /// </summary>
        private ObservableCollection<IPageViewModel> _pages = new ObservableCollection<IPageViewModel>();

        /// <summary>
        /// Create an instance of <see cref="PagesViewModel"/>
        /// </summary>
        /// <param name="applicationSettingsViewModel">Application settings</param>
        /// <param name="aboutViewModel">About</param>
        /// <param name="helpViewModel">Help</param>
        public SettingsPageViewModel(ApplicationSettingsViewModel applicationSettingsViewModel, AboutViewModel aboutViewModel, HelpViewModel helpViewModel)
        {
            applicationSettingsViewModel.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("OptionsLabel");
            aboutViewModel.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("AboutLabel");
            helpViewModel.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("HelpLabel");
            Pages = new ObservableCollection<IPageViewModel>
            {
                applicationSettingsViewModel,
                aboutViewModel,
                helpViewModel
            };

            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                message =>
                {
                    foreach (var page in Pages)
                    {
                        if (page is ApplicationSettingsViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("OptionsLabel");
                        }
                        else if (page is AboutViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("AboutLabel");
                        }
                        else if (page is HelpViewModel)
                        {
                            page.Caption = LocalizationProviderHelper.GetLocalizedValue<string>("HelpLabel");
                        }
                    }
                });
        }

        /// <summary>
        /// Pages shown into the interface
        /// </summary>
        public ObservableCollection<IPageViewModel> Pages
        {
            get => _pages;
            private set { Set(() => Pages, ref _pages, value); }
        }

        /// <summary>
        /// Tab caption 
        /// </summary>
        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }
    }
}
