using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Akavache;
using Enterwell.Clients.Wpf.Notifications;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NLog;
using OSDB.Models;
using Polly.Timeout;
using Popcorn.Dialogs;
using Popcorn.Enums;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Services.Application;
using Popcorn.Services.Cache;
using Popcorn.Services.User;
using Popcorn.Utils;
using Popcorn.Exceptions;
using Popcorn.ViewModels.Dialogs;
using Popcorn.ViewModels.Pages.Player;
using Popcorn.Services.Subtitles;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;

namespace Popcorn.ViewModels.Windows
{
    /// <summary>
    /// Window applcation's viewmodel
    /// </summary>
    public class WindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds the async message relative to <see cref="ShowCustomSubtitleMessage"/>
        /// </summary>
        private IDisposable _customSubtitleMessage;

        /// <summary>
        /// Holds the async message relative to <see cref="ShowLicenseDialogMessage"/>
        /// </summary>
        private IDisposable _showLicenseDialogMessage;

        /// <summary>
        /// Holds the async message relative to <see cref="ShowDownloadSettingsDialogMessage"/>
        /// </summary>
        private IDisposable _showDownloadSettingsMessage;

        /// <summary>
        /// Used to define the dialog context
        /// </summary>
        private readonly IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Specify if movie flyout is open
        /// </summary>
        private bool _isMovieFlyoutOpen;

        /// <summary>
        /// Specify if cast flyout is open
        /// </summary>
        private bool _isCastFlyoutOpen;

        /// <summary>
        /// Specify if show flyout is open
        /// </summary>
        private bool _isShowFlyoutOpen;

        /// <summary>
        /// Toggle fullscreen mode
        /// </summary>
        private bool _toggleFullscreen;

        /// <summary>
        /// Application state
        /// </summary>
        private IApplicationService _applicationService;

        /// <summary>
        /// Subtitles service
        /// </summary>
        private readonly ISubtitlesService _subtitlesService;

        /// <summary>
        /// The movie history service
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// <see cref="MediaPlayer"/>
        /// </summary>
        private MediaPlayerViewModel _mediaPlayer;

        /// <summary>
        /// Ignore taskbar on maximize
        /// </summary>
        private bool _ignoreTaskbarOnMaximize;

        /// <summary>
        /// Main frame navigation service
        /// </summary>
        public NavigationService NavigationService { get; set; }

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly NotificationMessageManager _manager;

        /// <summary>
        /// The cache service
        /// </summary>
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Semaphore for notifications
        /// </summary>
        private readonly SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the WindowViewModel class.
        /// </summary>
        /// <param name="applicationService">Instance of Application state</param>
        /// <param name="userService">Instance of movie history service</param>
        /// <param name="subtitlesService">Instance of subtitles service</param>
        /// <param name="cacheService">Instance of cache service</param>
        /// <param name="manager">The notification manager</param>
        public WindowViewModel(IApplicationService applicationService, IUserService userService,
            ISubtitlesService subtitlesService,
            ICacheService cacheService, NotificationMessageManager manager)
        {
            _cacheService = cacheService;
            _manager = manager;
            _subtitlesService = subtitlesService;
            _userService = userService;
            _dialogCoordinator = DialogCoordinator.Instance;
            _applicationService = applicationService;
            RegisterMessages();
            RegisterCommands();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
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
        /// Ignore taskbar on maximize
        /// </summary>
        public bool IgnoreTaskbarOnMaximize
        {
            get => _ignoreTaskbarOnMaximize;
            set { Set(() => IgnoreTaskbarOnMaximize, ref _ignoreTaskbarOnMaximize, value); }
        }

        /// <summary>
        /// Specify if movie flyout is open
        /// </summary>
        public bool IsMovieFlyoutOpen
        {
            get => _isMovieFlyoutOpen;
            set { Set(() => IsMovieFlyoutOpen, ref _isMovieFlyoutOpen, value); }
        }

        /// <summary>
        /// Specify if cast flyout is open
        /// </summary>
        public bool IsCastFlyoutOpen
        {
            get => _isCastFlyoutOpen;
            set { Set(() => IsCastFlyoutOpen, ref _isCastFlyoutOpen, value); }
        }

        /// <summary>
        /// Specify if show flyout is open
        /// </summary>
        public bool IsShowFlyoutOpen
        {
            get => _isShowFlyoutOpen;
            set { Set(() => IsShowFlyoutOpen, ref _isShowFlyoutOpen, value); }
        }

        /// <summary>
        /// Media player
        /// </summary>
        public MediaPlayerViewModel MediaPlayer
        {
            get => _mediaPlayer;
            set { Set(() => MediaPlayer, ref _mediaPlayer, value); }
        }

        /// <summary>
        /// Command used to close movie page
        /// </summary>
        public ICommand CloseMoviePageCommand { get; private set; }

        /// <summary>
        /// Command used to close cast page
        /// </summary>
        public ICommand CloseCastPageCommand { get; private set; }

        /// <summary>
        /// Command used to close show page
        /// </summary>
        public ICommand CloseShowPageCommand { get; private set; }

        /// <summary>
        /// Command used to close the application
        /// </summary>
        public ICommand MainWindowClosingCommand { get; private set; }

        /// <summary>
        /// Command used to switch fullscreen mode
        /// </summary>
        public ICommand SwitchFullScreenCommand { get; private set; }

        /// <summary>
        /// Command used to load tabs
        /// </summary>
        public ICommand InitializeAsyncCommand { get; private set; }

        /// <summary>
        /// Command used to drop files
        /// </summary>
        public ICommand DropFileCommand { get; private set; }

        /// <summary>
        /// Command used to manage drag enter
        /// </summary>
        public ICommand DragEnterFileCommand { get; private set; }

        /// <summary>
        /// Command used to manage drag leave
        /// </summary>
        public ICommand DragLeaveFileCommand { get; private set; }

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<ManageExceptionMessage>(this,
                async e => await ManageException(e.UnHandledException));

            Messenger.Default.Register<LoadMovieMessage>(this, e =>
            {
                IsCastFlyoutOpen = false;
                IsMovieFlyoutOpen = true;
            });

            Messenger.Default.Register<LoadShowMessage>(this, e =>
            {
                IsCastFlyoutOpen = false;
                IsShowFlyoutOpen = true;
            });

            Messenger.Default.Register<PlayShowEpisodeMessage>(this, message =>
            {
                MediaPlayer = new MediaPlayerViewModel(_subtitlesService, _cacheService,
                    message.Episode.FilePath,
                    message.Episode.Title,
                    MediaType.Show,
                    () => { Messenger.Default.Send(new StopPlayingEpisodeMessage()); },
                    message.PlayingProgress,
                    message.BufferProgress,
                    message.BandwidthRate,
                    message.Episode.SelectedSubtitle,
                    message.Episode.AvailableSubtitles);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ApplicationService.IsMediaPlaying = true;
                    IsShowFlyoutOpen = false;
                    if (NavigationService.CurrentSource.OriginalString == "Popcorn;component/Pages/PlayerPage.xaml")
                    {
                        NavigationService.Refresh();
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("Popcorn;component/Pages/PlayerPage.xaml",
                            UriKind.Relative));
                    }

                    IgnoreTaskbarOnMaximize = true;
                });
            });

            Messenger.Default.Register<PlayMediaMessage>(this, message =>
            {
                MediaPlayer = new MediaPlayerViewModel(_subtitlesService, _cacheService,
                    message.MediaPath,
                    message.MediaPath,
                    MediaType.Unkown,
                    () => { Messenger.Default.Send(new NavigateToHomePageMessage()); },
                    message.PlayingProgress,
                    message.BufferProgress,
                    message.BandwidthRate, subtitles: new List<Subtitle>
                    {
                        new Subtitle
                        {
                            LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"),
                        },
                        new Subtitle
                        {
                            LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"),
                        }
                    });

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ApplicationService.IsMediaPlaying = true;
                    IsShowFlyoutOpen = false;
                    IsMovieFlyoutOpen = false;
                    IsCastFlyoutOpen = false;
                    if (NavigationService.CurrentSource.OriginalString == "Popcorn;component/Pages/PlayerPage.xaml")
                    {
                        NavigationService.Refresh();
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("Popcorn;component/Pages/PlayerPage.xaml",
                            UriKind.Relative));
                    }

                    IgnoreTaskbarOnMaximize = true;
                });
            });

            Messenger.Default.Register<PlayMovieMessage>(this, message =>
            {
                MediaPlayer = new MediaPlayerViewModel(_subtitlesService, _cacheService,
                    message.Movie.FilePath, message.Movie.Title,
                    MediaType.Movie,
                    () =>
                    {
                        _userService.SetMovie(message.Movie);
                        Messenger.Default.Send(new ChangeSeenMovieMessage());
                        Messenger.Default.Send(new StopPlayingMovieMessage());
                    },
                    message.PlayingProgress,
                    message.BufferProgress,
                    message.BandwidthRate,
                    message.Movie.SelectedSubtitle,
                    message.Movie.AvailableSubtitles);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ApplicationService.IsMediaPlaying = true;
                    IsMovieFlyoutOpen = false;
                    IsShowFlyoutOpen = false;
                    IsCastFlyoutOpen = false;
                    if (NavigationService.CurrentSource.OriginalString == "Popcorn;component/Pages/PlayerPage.xaml")
                    {
                        NavigationService.Refresh();
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("Popcorn;component/Pages/PlayerPage.xaml",
                            UriKind.Relative));
                    }

                    IgnoreTaskbarOnMaximize = true;
                });
            });

            Messenger.Default.Register<PlayTrailerMessage>(this, message =>
            {
                MediaPlayer = new MediaPlayerViewModel(_subtitlesService, _cacheService,
                    message.TrailerUrl,
                    message.MovieTitle,
                    MediaType.Trailer, message.TrailerStoppedAction);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ApplicationService.IsMediaPlaying = true;
                    IsMovieFlyoutOpen = false;
                    IsShowFlyoutOpen = false;
                    IsCastFlyoutOpen = false;
                    if (NavigationService.CurrentSource.OriginalString == "Popcorn;component/Pages/PlayerPage.xaml")
                    {
                        NavigationService.Refresh();
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("Popcorn;component/Pages/PlayerPage.xaml",
                            UriKind.Relative));
                    }

                    IgnoreTaskbarOnMaximize = true;
                });
            });

            Messenger.Default.Register<StopPlayingTrailerMessage>(this, message =>
            {
                if (!_toggleFullscreen)
                    IgnoreTaskbarOnMaximize = false;
                if (ApplicationService.IsMediaPlaying)
                {
                    if (!_toggleFullscreen && ApplicationService.IsFullScreen)
                    {
                        ApplicationService.IsFullScreen = false;
                        ApplicationService.IsFullScreen = true;
                    }

                    ApplicationService.IsMediaPlaying = false;
                    if (message.Type == MediaType.Movie)
                    {
                        IsMovieFlyoutOpen = true;
                    }
                    else
                    {
                        IsShowFlyoutOpen = true;
                    }
                }
            });

            Messenger.Default.Register<StopPlayMediaMessage>(this, message =>
            {
                if (!_toggleFullscreen)
                {
                    IgnoreTaskbarOnMaximize = false;
                    if (ApplicationService.IsMediaPlaying && ApplicationService.IsFullScreen)
                    {
                        ApplicationService.IsFullScreen = false;
                        ApplicationService.IsFullScreen = true;
                    }
                }

                ApplicationService.IsMediaPlaying = false;
            });

            Messenger.Default.Register<NavigateToHomePageMessage>(this, message =>
            {
                if (!_toggleFullscreen)
                {
                    IgnoreTaskbarOnMaximize = false;
                    if (ApplicationService.IsMediaPlaying && ApplicationService.IsFullScreen)
                    {
                        ApplicationService.IsFullScreen = false;
                        ApplicationService.IsFullScreen = true;
                    }
                }

                ApplicationService.IsMediaPlaying = false;
                IsMovieFlyoutOpen = false;
                IsCastFlyoutOpen = false;
                IsShowFlyoutOpen = false;

                if (NavigationService.CurrentSource.OriginalString != "Pages/HomePage.xaml" &&
                    NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            });

            Messenger.Default.Register<StopPlayingEpisodeMessage>(
                this,
                message =>
                {
                    if (!_toggleFullscreen)
                    {
                        IgnoreTaskbarOnMaximize = false;
                        if (ApplicationService.IsMediaPlaying && ApplicationService.IsFullScreen)
                        {
                            ApplicationService.IsFullScreen = false;
                            ApplicationService.IsFullScreen = true;
                        }
                    }

                    ApplicationService.IsMediaPlaying = false;
                    IsShowFlyoutOpen = true;
                });

            Messenger.Default.Register<StopPlayingMovieMessage>(
                this,
                message =>
                {
                    if (!_toggleFullscreen)
                    {
                        IgnoreTaskbarOnMaximize = false;
                        if (ApplicationService.IsMediaPlaying && ApplicationService.IsFullScreen)
                        {
                            ApplicationService.IsFullScreen = false;
                            ApplicationService.IsFullScreen = true;
                        }
                    }

                    ApplicationService.IsMediaPlaying = false;
                    IsMovieFlyoutOpen = true;
                });

            Messenger.Default.Register<UnhandledExceptionMessage>(this,
                async message => await ManageException(message.Exception));

            Messenger.Default.Register<SearchCastMessage>(this, message => { IsCastFlyoutOpen = true; });

            Messenger.Default.Register<DownloadMagnetLinkMessage>(this,
                async message => { await HandleTorrentDownload(message.MagnetLink); });

            _showLicenseDialogMessage = Messenger.Default.RegisterAsyncMessage<ShowLicenseDialogMessage>(
                async message =>
                {
                    var vm = new LicenseDialogViewModel();
                    var licenseDialog = new LicenseDialog
                    {
                        DataContext = vm
                    };

                    var cts = new TaskCompletionSource<object>();
                    vm.OnCloseAction = async () =>
                    {
                        try
                        {
                            var dialog = await _dialogCoordinator.GetCurrentDialogAsync<LicenseDialog>(
                                this);
                            if (dialog != null)
                                await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                            cts.TrySetResult(null);
                        }
                        catch (Exception ex)
                        {
                            cts.TrySetException(ex);
                        }
                    };

                    try
                    {
                        await _dialogCoordinator.ShowMetroDialogAsync(this, licenseDialog, new MetroDialogSettings
                        {
                            ColorScheme = MetroDialogColorScheme.Accented
                        });
                        await cts.Task;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });

            _showDownloadSettingsMessage = Messenger.Default.RegisterAsyncMessage<ShowDownloadSettingsDialogMessage>(
                async message =>
                {
                    var vm = new DownloadSettingsDialogViewModel(message.Media, _subtitlesService);
                    var subtitleDialog = new DownloadSettingsDialog
                    {
                        DataContext = vm
                    };

                    var cts = new TaskCompletionSource<object>();
                    vm.OnCloseAction = async result =>
                    {
                        try
                        {
                            message.Download = result;
                            var dialog = await _dialogCoordinator.GetCurrentDialogAsync<DownloadSettingsDialog>(
                                this);
                            if (dialog != null)
                                await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                            cts.TrySetResult(null);
                        }
                        catch (Exception ex)
                        {
                            cts.TrySetException(ex);
                        }
                    };

                    try
                    {
                        await _dialogCoordinator.ShowMetroDialogAsync(this, subtitleDialog, new MetroDialogSettings
                        {
                            ColorScheme = MetroDialogColorScheme.Accented
                        });
                        await cts.Task;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });

            _customSubtitleMessage = Messenger.Default.RegisterAsyncMessage<ShowCustomSubtitleMessage>(
                async message =>
                {
                    var fileDialog = new OpenFileDialog
                    {
                        Title = "Open Sub File",
                        Filter = "SUB files (*.sub,*srt,*sbv)|*.sub;*.srt;*.sbv",
                        InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory)
                    };

                    if (fileDialog.ShowDialog() == true)
                    {
                        try
                        {
                            message.FileName = fileDialog.FileName;
                            await Task.FromResult(message.FileName);
                        }
                        catch (Exception)
                        {
                            message.Error = true;
                        }
                    }
                });
        }

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            CloseMoviePageCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                IsMovieFlyoutOpen = false;
                if (NavigationService.CurrentSource.OriginalString != "Pages/HomePage.xaml" &&
                    NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            });

            CloseShowPageCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                IsShowFlyoutOpen = false;
                if (NavigationService.CurrentSource.OriginalString != "Pages/HomePage.xaml" &&
                    NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            });

            CloseCastPageCommand = new RelayCommand(() =>
            {
                IsCastFlyoutOpen = false;
            });

            MainWindowClosingCommand = new RelayCommand(async () =>
            {
                try
                {
                    FileHelper.ClearFolders();
                    await _userService.UpdateUser();
                    await SaveCacheOnExit();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });

            DropFileCommand = new RelayCommand<DragEventArgs>(async e =>
            {
                try
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                        var torrentFile = files?.FirstOrDefault(a => a.Contains("torrent") || a.Contains("magnet"));
                        if (torrentFile != null)
                        {
                            var vm = new DropTorrentDialogViewModel(_cacheService, torrentFile);
                            var dropTorrentDialog = new DropTorrentDialog
                            {
                                DataContext = vm
                            };

                            Messenger.Default.Send(new DropFileMessage(DropFileMessage.DropFileEvent.Leave));
                            await _dialogCoordinator.ShowMetroDialogAsync(this, dropTorrentDialog);
                            var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                            await vm.Download(settings.UploadLimit, settings.DownloadLimit,
                                async () =>
                                {
                                    try
                                    {
                                        var dialog =
                                            await _dialogCoordinator.GetCurrentDialogAsync<DropTorrentDialog>(
                                                this);
                                        if (dialog != null)
                                            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex);
                                    }
                                }, async () =>
                                {
                                    try
                                    {
                                        var dialog =
                                            await _dialogCoordinator.GetCurrentDialogAsync<DropTorrentDialog>(
                                                this);
                                        if (dialog != null)
                                            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex);
                                    }
                                });
                        }

                        var subtitleFile = files?.FirstOrDefault(a =>
                            a.Contains(".sub") || a.Contains(".srt") || a.Contains(".sbv"));
                        if (subtitleFile != null)
                        {
                            MediaPlayer?.ChangeSubtitle(new Subtitle
                            {
                                FilePath = subtitleFile,
                                LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel")
                            });
                        }
                    }
                    else
                    {
                        Messenger.Default.Send(
                            new UnhandledExceptionMessage(
                                new NoDataInDroppedFileException(
                                    LocalizationProviderHelper.GetLocalizedValue<string>("NoMediaInDroppedTorrent"))));
                    }
                }
                catch (Exception)
                {
                    Messenger.Default.Send(
                        new UnhandledExceptionMessage(
                            new PopcornException(
                                LocalizationProviderHelper.GetLocalizedValue<string>("DroppedFileIssue"))));
                }
                finally
                {
                    Messenger.Default.Send(new DropFileMessage(DropFileMessage.DropFileEvent.Leave));
                }
            });

            SwitchFullScreenCommand = new RelayCommand(() =>
            {
                ToggleFullscren = !ToggleFullscren;
                if (ApplicationService.IsFullScreen && !IgnoreTaskbarOnMaximize)
                    IgnoreTaskbarOnMaximize = true;
                else if (ApplicationService.IsFullScreen && IgnoreTaskbarOnMaximize)
                {
                    IgnoreTaskbarOnMaximize = false;
                    ApplicationService.IsFullScreen = false;
                }
                else if (!ApplicationService.IsFullScreen && IgnoreTaskbarOnMaximize)
                {
                    IgnoreTaskbarOnMaximize = false;
                }
                else
                {
                    IgnoreTaskbarOnMaximize = true;
                    ApplicationService.IsFullScreen = true;
                }
            });

            DragEnterFileCommand = new RelayCommand<DragEventArgs>(e =>
            {
                Messenger.Default.Send(new DropFileMessage(DropFileMessage.DropFileEvent.Enter));
            });

            DragLeaveFileCommand = new RelayCommand<DragEventArgs>(e =>
            {
                Messenger.Default.Send(new DropFileMessage(DropFileMessage.DropFileEvent.Leave));
            });

            InitializeAsyncCommand = new RelayCommand(async () =>
            {
                var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                await settings.InitializeAsync();
                var cmd = Environment.GetCommandLineArgs();
                if (cmd.Any())
                {
                    if (cmd.Contains("updated"))
                    {
                        // TODO: show info about new update
                    }
                    else if (cmd.Length == 2 && (cmd[1].StartsWith("magnet") || cmd[1].EndsWith("torrent")))
                    {
                        var path = cmd[1];
                        await HandleTorrentDownload(path);
                    }
                }
            });
        }

        private async Task HandleTorrentDownload(string path)
        {
            var filePath = path;
            var vm = new DropTorrentDialogViewModel(_cacheService, filePath);
            var dropTorrentDialog = new DropTorrentDialog
            {
                DataContext = vm
            };

            try
            {
                await _dialogCoordinator.ShowMetroDialogAsync(this, dropTorrentDialog);
                var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                await vm.Download(settings.UploadLimit, settings.DownloadLimit,
                    async () =>
                    {
                        var dialog = await _dialogCoordinator.GetCurrentDialogAsync<DropTorrentDialog>(
                            this);
                        if (dialog != null)
                            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                    },
                    async () =>
                    {
                        var dialog = await _dialogCoordinator.GetCurrentDialogAsync<DropTorrentDialog>(
                            this);
                        if (dialog != null)
                            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool ToggleFullscren
        {
            get => _toggleFullscreen;
            set => Set(ref _toggleFullscreen, value);
        }

        /// <summary>
        /// Flush cache on disk
        /// </summary>
        private async Task SaveCacheOnExit()
        {
            await BlobCache.Shutdown();
        }

        /// <summary>
        /// Display a dialog on unhandled exception
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        /// <summary>
        /// Manage an exception
        /// </summary>
        /// <param name="exception">The exception to manage</param>
        private async Task ManageException(Exception exception)
        {
            if (_notificationSemaphore.CurrentCount == 0)
                return;

            await _notificationSemaphore.WaitAsync();
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (exception is WebException || exception is SocketException || exception is TimeoutRejectedException)
                {
                    _applicationService.IsConnectionInError = true;
                    _manager.CreateMessage()
                        .Animates(true)
                        .Accent("#E82C0C")
                        .Background("#333")
                        .HasBadge("Error")
                        .HasMessage(
                            LocalizationProviderHelper.GetLocalizedValue<string>("ConnectionErrorDescriptionPopup"))
                        .Dismiss().WithButton(LocalizationProviderHelper.GetLocalizedValue<string>("Ignore"),
                            button => { _notificationSemaphore.Release(); })
                        .Queue();
                }
                else if (exception is TrailerNotAvailableException)
                {
                    _manager.CreateMessage()
                        .Animates(true)
                        .Accent("#E0A030")
                        .Background("#333")
                        .HasBadge("Warning")
                        .HasMessage(exception.Message)
                        .Dismiss().WithButton(LocalizationProviderHelper.GetLocalizedValue<string>("Dismiss"),
                            button => { _notificationSemaphore.Release(); })
                        .Queue();
                }
                else if (exception is NoDataInDroppedFileException)
                {
                    _manager.CreateMessage()
                        .Animates(true)
                        .Accent("#E0A030")
                        .Background("#333")
                        .HasBadge("Warning")
                        .HasMessage(exception.Message)
                        .Dismiss().WithButton(LocalizationProviderHelper.GetLocalizedValue<string>("Dismiss"),
                            button => { _notificationSemaphore.Release(); })
                        .Queue();
                }
                else if (exception is PopcornException)
                {
                    _manager.CreateMessage()
                        .Animates(true)
                        .Accent("#E82C0C")
                        .Background("#333")
                        .HasBadge("Error")
                        .HasMessage(exception.Message)
                        .Dismiss().WithButton(LocalizationProviderHelper.GetLocalizedValue<string>("Ignore"),
                            button => { _notificationSemaphore.Release(); })
                        .Queue();
                }
                else
                {
                    Logger.Fatal(exception);
                }
            });
        }
    }
}