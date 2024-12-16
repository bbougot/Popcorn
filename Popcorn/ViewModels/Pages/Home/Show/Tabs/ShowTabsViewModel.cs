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
using Popcorn.Models.Shows;
using Popcorn.Services.Application;
using Popcorn.Services.Shows.Show;
using Popcorn.Services.User;

namespace Popcorn.ViewModels.Pages.Home.Show.Tabs
{
    /// <summary>
    /// Manage tab controls
    /// </summary>
    public class ShowTabsViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Need sync shows
        /// </summary>
        public bool NeedSync;

        /// <summary>
        /// Semaphore loading
        /// </summary>
        protected readonly SemaphoreSlim LoadingSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The genre used to filter shows
        /// </summary>
        private GenreJson _genre;

        /// <summary>
        /// The rating used to filter shows
        /// </summary>
        private double _rating;

        /// <summary>
        /// Vertical scroll offset
        /// </summary>
        private double _verticalScroll;

        /// <summary>
        /// Services used to interact with movie history
        /// </summary>
        protected readonly IUserService UserService;

        /// <summary>
        /// Services used to interact with shows
        /// </summary>
        protected readonly IShowService ShowService;

        /// <summary>
        /// The current number of shows of the tab
        /// </summary>
        private int _currentNumberOfShows;

        /// <summary>
        /// Specify if a show loading has failed
        /// </summary>
        private bool _hasLoadingFailed;

        /// <summary>
        /// Specify if shows are loading
        /// </summary>
        private bool _isLoadingShows;

        /// <summary>
        /// Indicates if there's any show found
        /// </summary>
        private bool _isShowsFound = true;

        /// <summary>
        /// The maximum number of shows found
        /// </summary>
        private int _maxNumberOfShows;

        /// <summary>
        /// The tab's shows
        /// </summary>
        private ObservableCollection<ShowLightJson> _shows = new ObservableCollection<ShowLightJson>();

        /// <summary>
        /// The tab's name
        /// </summary>
        private string _tabName;

        /// <summary>
        /// Func which generates the tab name
        /// </summary>
        private readonly Func<string> _tabNameGenerator;

        /// <summary>
        /// Initializes a new instance of the ShowTabsViewModel class.
        /// </summary>
        /// <param name="applicationService">The application state</param>
        /// <param name="showService">Used to interact with shows</param>
        /// <param name="userService">THe user service</param>
        /// <param name="tabNameGenerator">Func which generates the tab name</param>
        protected ShowTabsViewModel(IApplicationService applicationService, IShowService showService,
            IUserService userService, Func<string> tabNameGenerator)
        {
            ApplicationService = applicationService;
            ShowService = showService;
            UserService = userService;

            RegisterMessages();
            RegisterCommands();

            _tabNameGenerator = tabNameGenerator;
            TabName = tabNameGenerator.Invoke();
            MaxShowsPerPage = Helpers.Constants.MaxShowsPerPage;
            CancellationLoadingShows = new CancellationTokenSource();
        }

        /// <summary>
        /// Application state
        /// </summary>
        public IApplicationService ApplicationService { get; }

        /// <summary>
        /// Tab's shows
        /// </summary>
        public ObservableCollection<ShowLightJson> Shows
        {
            get => _shows;
            set { Set(() => Shows, ref _shows, value); }
        }

        /// <summary>
        /// The current number of shows in the tab
        /// </summary>
        public int CurrentNumberOfShows
        {
            get => _currentNumberOfShows;
            set { Set(() => CurrentNumberOfShows, ref _currentNumberOfShows, value); }
        }

        /// <summary>
        /// The maximum number of shows found
        /// </summary>
        public int MaxNumberOfShows
        {
            get => _maxNumberOfShows;
            set { Set(() => MaxNumberOfShows, ref _maxNumberOfShows, value); }
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
        /// Specify if shows are loading
        /// </summary>
        public bool IsLoadingShows
        {
            get => _isLoadingShows;
            protected set { Set(() => IsLoadingShows, ref _isLoadingShows, value); }
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
        /// Indicates if there's any show found
        /// </summary>
        public bool IsShowFound
        {
            get => _isShowsFound;
            set { Set(() => IsShowFound, ref _isShowsFound, value); }
        }

        /// <summary>
        /// The rating used to filter shows
        /// </summary>
        public double Rating
        {
            get => _rating;
            set { Set(() => Rating, ref _rating, value, true); }
        }

        /// <summary>
        /// Command used to reload shows
        /// </summary>
        public ICommand ReloadShows { get; set; }

        /// <summary>
        /// Command used to set a show as favorite
        /// </summary>
        public RelayCommand<ShowLightJson> SetFavoriteShowCommand { get; private set; }

        /// <summary>
        /// Command used to change show's genres
        /// </summary>
        public RelayCommand<GenreJson> ChangeShowGenreCommand { get; set; }

        /// <summary>
        /// Specify if a show loading has failed
        /// </summary>
        public bool HasLoadingFailed
        {
            get => _hasLoadingFailed;
            set { Set(() => HasLoadingFailed, ref _hasLoadingFailed, value); }
        }

        /// <summary>
        /// The genre used to filter shows
        /// </summary>
        public GenreJson Genre
        {
            get => _genre;
            private set { Set(() => Genre, ref _genre, value, true); }
        }

        /// <summary>
        /// Current page number of loaded shows
        /// </summary>
        protected int Page { get; set; }

        /// <summary>
        /// Maximum shows number to load per page request
        /// </summary>
        protected int MaxShowsPerPage { get; set; }

        /// <summary>
        /// Token to cancel show loading
        /// </summary>
        protected CancellationTokenSource CancellationLoadingShows { get; private set; }

        /// <summary>
        /// Sort by
        /// </summary>
        protected string SortBy { get; set; }

        /// <summary>
        /// The selected tab
        /// </summary>
        public static ShowTabsViewModel SelectedTab { get; set; }

        /// <summary>
        /// Load shows asynchronously
        /// </summary>
        public virtual async Task LoadShowsAsync(bool reset = false)
        {
            await LoadingSemaphore.WaitAsync(CancellationLoadingShows.Token);
            await Task.Run(async () =>
            {
                if (reset)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Shows.Clear();
                        Page = 0;
                        VerticalScroll = 0d;
                    });
                }

                var watch = Stopwatch.StartNew();
                Page++;
                if (Page > 1 && Shows.Count == MaxNumberOfShows)
                {
                    Page--;
                    LoadingSemaphore.Release();
                    return;
                }

                StopLoadingShows();
                Logger.Trace(
                    $"Loading page {Page}...");
                HasLoadingFailed = false;
                try
                {
                    IsLoadingShows = true;
                    var getShowWatcher = new Stopwatch();
                    var result =
                        await ShowService.GetShowsAsync(Page,
                            MaxShowsPerPage,
                            Rating * 10,
                            SortBy,
                            CancellationLoadingShows.Token,
                            Genre);
                    getShowWatcher.Stop();
                    var getShowEllapsedTime = getShowWatcher.ElapsedMilliseconds;
                    if (reset && getShowEllapsedTime < 500)
                    {
                        // Wait for VerticalOffset to reach 0 (animation lasts 500ms)
                        await Task.Delay(500 - (int) getShowEllapsedTime);
                    }

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Shows.AddRange(result.shows.Except(Shows, new ShowLightComparer()));
                        IsLoadingShows = false;
                        IsShowFound = Shows.Any();
                        CurrentNumberOfShows = Shows.Count;
                        MaxNumberOfShows = result.nbShows == 0 ? Shows.Count : result.nbShows;
                        UserService.SyncShowHistory(Shows);
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
            StopLoadingShows();
            base.Cleanup();
        }

        /// <summary>
        /// Cancel the loading of the next page
        /// </summary>
        protected void StopLoadingShows()
        {
            CancellationLoadingShows.Cancel();
            CancellationLoadingShows.Dispose();
            CancellationLoadingShows = new CancellationTokenSource();
        }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<ChangeFavoriteShowMessage>(
                this,
                async message =>
                {
                    UserService.SyncShowHistory(Shows);
                    if (this is FavoritesShowTabViewModel && !(SelectedTab is FavoritesShowTabViewModel))
                        NeedSync = true;
                    else if (this is FavoritesShowTabViewModel && SelectedTab is FavoritesShowTabViewModel)
                    {
                        NeedSync = true;
                        await LoadShowsAsync();
                    }
                });

            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                language => TabName = _tabNameGenerator.Invoke());

            Messenger.Default.Register<PropertyChangedMessage<GenreJson>>(this, async e =>
            {
                if (e.PropertyName != GetPropertyName(() => Genre) ||
                    e.PropertyName != GetPropertyName(() => Genre) && Genre.Equals(e.NewValue) ||
                    !(e.Sender is ShowTabsViewModel)) return;
                _genre = e.NewValue;
                if (SelectedTab == this)
                {
                    await LoadShowsAsync(true);
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
                    !(e.Sender is ShowTabsViewModel)) return;
                _rating = e.NewValue;
                if (SelectedTab == this)
                {
                    await LoadShowsAsync(true);
                }
                else
                {
                    NeedSync = true;
                }
            });
        }

        /// <summary>
        /// Register commands
        /// </summary>
        /// <returns></returns>
        private void RegisterCommands()
        {
            SetFavoriteShowCommand =
                new RelayCommand<ShowLightJson>(show =>
                {
                    UserService.SetShow(show);
                    Messenger.Default.Send(new ChangeFavoriteShowMessage());
                });

            ChangeShowGenreCommand =
                new RelayCommand<GenreJson>(genre => Genre = genre.Name ==
                                                             LocalizationProviderHelper.GetLocalizedValue<string>(
                                                                 "AllLabel")
                    ? null
                    : genre);

            ReloadShows = new RelayCommand(async () =>
            {
                ApplicationService.IsConnectionInError = false;
                await LoadShowsAsync();
            });
        }
    }
}