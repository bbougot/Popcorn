using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Messaging;

namespace Popcorn.ViewModels.Pages.Home.Show.Search
{
    /// <summary>
    /// Show's search
    /// </summary>
    public class SearchShowViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The filter for searching shows
        /// </summary>
        private string _searchFilter;

        /// <summary>
        /// Initializes a new instance of the SearchShowViewModel class.
        /// </summary>
        public SearchShowViewModel()
        {
            RegisterMessages();
            RegisterCommands();
        }

        /// <summary>
        /// The filter for searching shows
        /// </summary>
        public string SearchFilter
        {
            get => _searchFilter;
            set { Set(() => SearchFilter, ref _searchFilter, value, true); }
        }

        /// <summary>
        /// Command used to search shows
        /// </summary>
        public RelayCommand SearchCommand { get; private set; }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages() => Messenger.Default.Register<PropertyChangedMessage<string>>(this, e =>
        {
            if (e.PropertyName == GetPropertyName(() => SearchFilter) && string.IsNullOrEmpty(e.NewValue))
                Messenger.Default.Send(new SearchShowMessage(string.Empty));
        });

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands() => SearchCommand =
            new RelayCommand(() =>
            {
                Messenger.Default.Send(new SearchShowMessage(SearchFilter));
            });
    }
}
