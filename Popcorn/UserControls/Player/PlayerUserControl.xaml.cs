using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Popcorn.ViewModels.Pages.Player;
using System.Threading;
using System.Windows.Media;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Messaging;
using Popcorn.Models.Bandwidth;
using Popcorn.Services.Application;
using Popcorn.Utils;
using Popcorn.Exceptions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using GalaSoft.MvvmLight.CommandWpf;
using Popcorn.Converters;
using Unosquare.FFME;
using Unosquare.FFME.Common;

namespace Popcorn.UserControls.Player
{
    /// <summary>
    /// Interaction logic for PlayerUserControl.xaml
    /// </summary>
    public partial class PlayerUserControl : INotifyPropertyChanged
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Application service
        /// </summary>
        private readonly IApplicationService _applicationService;

        /// <summary>
        /// Used to update the activity mouse and mouse position.
        /// </summary>
        private DispatcherTimer ActivityTimer { get; set; }

        /// <summary>
        /// Get or set the mouse position when inactive
        /// </summary>
        private Point InactiveMousePosition { get; set; } = new Point(0, 0);

        /// <summary>
        /// Subtitle delay
        /// </summary>
        private int SubtitleDelay { get; set; }

        /// <summary>
        /// <see cref="SetLowerSubtitleSizeCommand"/>
        /// </summary>
        private ICommand _setLowerSubtitleSizeCommand;

        /// <summary>
        /// <see cref="SetHigherSubtitleSizeCommand"/>
        /// </summary>
        private ICommand _setHigherSubtitleSizeCommand;

        /// <summary>
        /// <see cref="PauseCommand"/>
        /// </summary>
        private ICommand _pauseCommand;

        /// <summary>
        /// <see cref="PlayCommand"/>
        /// </summary>
        private ICommand _playCommand;

        /// <summary>
        /// <see cref="BufferProgress"/>
        /// </summary>
        private double _bufferProgress;

        /// <summary>
        /// Semaphore used to update mouse activity
        /// </summary>
        private static readonly SemaphoreSlim MouseActivitySemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Semaphore used to update subtitle delay
        /// </summary>
        private static readonly SemaphoreSlim SubtitleDelaySemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the MoviePlayer class.
        /// </summary>
        public PlayerUserControl()
        {
            _applicationService = SimpleIoc.Default.GetInstance<IApplicationService>();
            Messenger.Default.Register<KeyPressedMessage>(this,
                async message => { await OnKeyPressed(message.KeyPressedArgs); });

            InitializeComponent();
            Loaded += OnLoaded;
            Media.MediaOpened += OnMediaOpened;
            Media.RendererOptions.UseLegacyAudioOut = true;
            MediaElement.FFmpegMessageLogged += OnMediaFFmpegMessageLogged;
            PauseCommand = new RelayCommand(async () => { await PauseMedia(); }, MediaPlayerPauseCanExecute);
            PlayCommand = new RelayCommand(async () => { await PlayMedia(); }, MediaPlayerPlayCanExecute);
        }

        /// <summary>
        /// Downloading buffer progress for movies/shows between 0 and 100
        /// </summary>
        public double BufferProgress
        {
            get => _bufferProgress;
            set
            {
                _bufferProgress = value;
                OnPropertyChanged(nameof(BufferProgress));
            }
        }

        /// <summary>
        /// Command used to lower subtitle size
        /// </summary>
        public ICommand SetLowerSubtitleSizeCommand
        {
            get => _setLowerSubtitleSizeCommand;
            set
            {
                _setLowerSubtitleSizeCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command used to lower subtitle size
        /// </summary>
        public ICommand SetHigherSubtitleSizeCommand
        {
            get => _setHigherSubtitleSizeCommand;
            set
            {
                _setHigherSubtitleSizeCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command used to pause media
        /// </summary>
        public ICommand PauseCommand
        {
            get => _pauseCommand;
            set
            {
                _pauseCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command used to play media
        /// </summary>
        public ICommand PlayCommand
        {
            get => _playCommand;
            set
            {
                _playCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// When user control has loaded, initialize the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += async (s1, e1) => await Unload();
            }

            var vm = DataContext as MediaPlayerViewModel;
            if (vm?.MediaPath == null)
                return;

            Subtitles.SetFontSize(Media, 22);
            Subtitles.SetForeground(Media, Brushes.White);
            ActivityTimer =
                new DispatcherTimer(DispatcherPriority.Background) {Interval = TimeSpan.FromSeconds(2)};
            ActivityTimer.Tick += OnInactivity;
            ActivityTimer.Start();
            InputManager.Current.PreProcessInput += OnActivity;
            SetLowerSubtitleSizeCommand = new RelayCommand(() =>
            {
                var currentSize = Subtitles.GetFontSize(Media);
                Subtitles.SetFontSize(Media, --currentSize);
            });

            SetHigherSubtitleSizeCommand = new RelayCommand(() =>
            {
                var currentSize = Subtitles.GetFontSize(Media);
                Subtitles.SetFontSize(Media, ++currentSize);
            });

            Subtitles.SetFontFamily(Media, new FontFamily("Verdana"));
            Subtitles.SetFontWeight(Media, FontWeights.Bold);
            Subtitles.SetOutlineWidth(Media, new Thickness(0, 0, 0, 0));
            vm.StoppedMedia += OnStoppedMedia;
            vm.SubtitleChanged += OnSubtitleChanged;
            if (vm.BufferProgress != null)
            {
                vm.BufferProgress.ProgressChanged += OnBufferProgressChanged;
            }

            if (vm.BandwidthRate != null)
            {
                vm.BandwidthRate.ProgressChanged += OnBandwidthChanged;
            }

            if (vm.MediaType == Enums.MediaType.Trailer)
            {
                DownloadProgress.Visibility = Visibility.Collapsed;
            }

            Title.Text = vm.MediaName;
            Media.MediaOpening += OnMediaOpening;
            Media.MediaChanging += OnMediaChanging;
            Media.MediaChanged += OnMediaChanged;
            await Media.Open(new Uri(vm.MediaPath));
        }

        private void OnMediaChanged(object sender, EventArgs e)
        {
        }

        private void OnMediaChanging(object sender, MediaOpeningEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                try
                {
                    if (!(DataContext is MediaPlayerViewModel vm))
                        return;

                    e.Options.SubtitlesDelay = TimeSpan.FromMilliseconds(SubtitleDelay);
                    if (string.IsNullOrEmpty(vm.CurrentSubtitle?.FilePath))
                    {
                        e.Options.SubtitlesSource = string.Empty;
                        return;
                    }

                    var url = new Uri(vm.CurrentSubtitle.FilePath);
                    if (url.IsFile || url.IsUnc)
                    {
                        if (System.IO.File.Exists(vm.CurrentSubtitle.FilePath) && e.Options.SubtitlesSource != vm.CurrentSubtitle.FilePath)
                        {
                            e.Options.SubtitlesSource = vm.CurrentSubtitle.FilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
        }

        private async void OnSubtitleChanged(object sender, Events.SubtitleChangedEventArgs e)
        {
            await Media.ChangeMedia();
        }

        /// <summary>
        /// Handle keys when pressed
        /// </summary>
        /// <param name="ke"></param>
        private async Task OnKeyPressed(KeyPressedArgs ke)
        {
            FocusManager.SetIsFocusScope(this, true);
            FocusManager.SetFocusedElement(this, this);

            if (ke.KeyPressed == Key.Space)
            {
                if (Media.IsPlaying)
                    await PauseMedia();
                else
                    await PlayMedia();
            }

            if (ke.KeyPressed == Key.Up)
            {
                Media.Volume += 0.05;
            }

            if (ke.KeyPressed == Key.Down)
            {
                Media.Volume -= 0.05;
            }

            if (ke.KeyPressed == Key.F)
            {
                _applicationService.IsFullScreen = !_applicationService.IsFullScreen;
            }

            if (!Media.HasSubtitles)
                return;

            switch (ke.KeyPressed)
            {
                case Key.H:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        SubtitleDelay += 100;
                    }
                    else
                    {
                        SubtitleDelay += 100;
                    }

                    break;
                case Key.G:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        SubtitleDelay -= 100;
                    }
                    else
                    {
                        SubtitleDelay -= 100;
                    }

                    break;
                default:
                    return;
            }

            Delay.Text = $"Subtitle delay: {SubtitleDelay} ms";
            await Media.ChangeMedia();
            if (SubtitleDelaySemaphore.CurrentCount == 0) return;
            await SubtitleDelaySemaphore.WaitAsync();
            var increasedPanelOpacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                KeyFrames = new DoubleKeyFrameCollection
                {
                    new EasingDoubleKeyFrame(0.6, KeyTime.FromPercent(1.0), new PowerEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    })
                }
            };
            var increasedSubtitleDelayOpacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                KeyFrames = new DoubleKeyFrameCollection
                {
                    new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0), new PowerEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    })
                }
            };

            SubtitleDelayPanel.BeginAnimation(OpacityProperty, increasedPanelOpacityAnimation);
            Delay.BeginAnimation(OpacityProperty, increasedSubtitleDelayOpacityAnimation);
            await Task.Delay(2000);
            var decreasedOpacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                KeyFrames = new DoubleKeyFrameCollection
                {
                    new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0), new PowerEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    })
                }
            };

            SubtitleDelayPanel.BeginAnimation(OpacityProperty, decreasedOpacityAnimation);
            Delay.BeginAnimation(OpacityProperty, decreasedOpacityAnimation);
            SubtitleDelaySemaphore.Release();
        }

        /// <summary>
        /// On mouse left button up, play/pause the media accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Media.IsPlaying)
                await PauseMedia();
            else
                await PlayMedia();
        }

        /// <summary>
        /// Subscribe to events and play the movie when control has been loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private async void OnMediaOpened(object sender, EventArgs e)
        {
            Media.MediaEnded += MediaPlayerEndReached;
            Media.MediaFailed += EncounteredError;
            Media.SeekingStarted += OnSeekingStarted;
            Media.SeekingEnded += OnSeekingEnded;
            await PlayMedia();
        }

        /// <summary>
        /// When a media is being opened, load the subtitles if any and initialize MediaLength property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMediaOpening(object sender, MediaOpeningEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                try
                {
                    if (!(DataContext is MediaPlayerViewModel vm))
                        return;

                    vm.MediaLength = e.Info.Duration.TotalSeconds;
                    if (string.IsNullOrEmpty(vm.CurrentSubtitle?.FilePath))
                        return;

                    var url = new Uri(vm.CurrentSubtitle.FilePath);
                    if (url.IsFile || url.IsUnc)
                    {
                        if (System.IO.File.Exists(vm.CurrentSubtitle.FilePath))
                            e.Options.SubtitlesSource = vm.CurrentSubtitle.FilePath;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
        }

        /// <summary>
        /// When seeking has started, show buffering panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSeekingStarted(object sender, EventArgs e)
        {
            if (!(DataContext is MediaPlayerViewModel vm))
                return;

            vm.IsSeeking = true;
            Buffering.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When seeking has ended, hide buffering panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSeekingEnded(object sender, EventArgs e)
        {
            if (!(DataContext is MediaPlayerViewModel vm))
                return;

            vm.IsSeeking = false;
            Buffering.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// When user has started dragging the media player slider, pause the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (!(DataContext is MediaPlayerViewModel vm))
                return;

            vm.IsDragging = true;
            await PauseMedia();
        }

        /// <summary>
        /// When user has stopped dragging the media player slider, play the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!(DataContext is MediaPlayerViewModel vm))
                return;

            vm.IsDragging = false;
            await PlayMedia();
        }

        /// <summary>
        /// When downloading buffer progress has changed, report it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bufferProgress"></param>
        private void OnBufferProgressChanged(object sender, double bufferProgress)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                BufferProgress = bufferProgress;
                BufferingSlider.Value = bufferProgress / 100d;
            });
        }

        /// <summary>
        /// When bandwidth rate has changed, report it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBandwidthChanged(object sender, BandwidthRate e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Download.Text = e.DownloadRate.ToString(CultureInfo.InvariantCulture);
                Upload.Text = e.UploadRate.ToString(CultureInfo.InvariantCulture);
                Remaining.Text = new TimeSpanFormatter().Convert(e.ETA, null, null, null).ToString();
            });
        }

        /// <summary>
        /// When the player has encountered an error, inform the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncounteredError(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Messenger.Default.Send(
                    new UnhandledExceptionMessage(
                        new PopcornException("An error has occured while trying to play the media.")));
                if (!(DataContext is MediaPlayerViewModel vm))
                    return;

                vm.MediaEnded();
            });
        }

        /// <summary>
        /// When user uses the mousewheel, update the volume
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MouseWheelEventArgs</param>
        private void MouseWheelMediaPlayer(object sender, MouseWheelEventArgs e)
        {
            Media.Volume += e.Delta > 0 ? 0.05 : -0.05;
        }

        /// <summary>
        /// When a movie has been seen, save this information in the user data
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void MediaPlayerEndReached(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (!(DataContext is MediaPlayerViewModel vm))
                    return;

                vm.MediaEnded();
            });
        }

        /// <summary>
        /// Play the movie
        /// </summary>
        private async Task PlayMedia()
        {
            try
            {
                _applicationService.SwitchConstantDisplayAndPower(true);
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Visible;
                if (DataContext is MediaPlayerViewModel vm && Media.NaturalDuration.HasValue)
                {
                    vm.MediaDuration = Media.NaturalDuration.Value.TotalSeconds;
                }

                await Media.Play();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Messenger.Default.Send(
                    new UnhandledExceptionMessage(
                        new PopcornException("An error has occured while trying to play the media.")));
                if (!(DataContext is MediaPlayerViewModel vm))
                    return;

                vm.MediaEnded();
            }
        }

        /// <summary>
        /// Pause the movie
        /// </summary>
        private async Task PauseMedia()
        {
            try
            {
                _applicationService.SwitchConstantDisplayAndPower(false);
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Visible;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
                await Media.Pause();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// When media has finished playing, dispose the control
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private async void OnStoppedMedia(object sender, EventArgs e) => await Unload();

        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the player
        /// </summary>
        private bool MediaPlayerPlayCanExecute()
        {
            if (Media.IsPlaying)
            {
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Visible;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
            }

            return !Media.IsPlaying;
        }

        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the media player
        /// </summary>
        private bool MediaPlayerPauseCanExecute()
        {
            if (Media.IsPlaying)
            {
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                MediaPlayerStatusBarItemPlay.Visibility = Visibility.Visible;
                MediaPlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
            }

            return Media.CanPause;
        }

        /// <summary>
        /// Hide the PlayerStatusBar on mouse inactivity
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnInactivity(object sender, EventArgs e)
        {
            if (InactiveMousePosition == Mouse.GetPosition(Container))
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = Cursors.None;
                }

                var opacityAnimation = new DoubleAnimationUsingKeyFrames
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    KeyFrames = new DoubleKeyFrameCollection
                    {
                        new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1d), new PowerEase
                        {
                            EasingMode = EasingMode.EaseInOut
                        })
                    }
                };

                PlayerStatusBar.BeginAnimation(OpacityProperty, opacityAnimation);
                UpperPanel.BeginAnimation(OpacityProperty, opacityAnimation);
            }

            InactiveMousePosition = Mouse.GetPosition(Container);
        }

        /// <summary>
        /// Show the PlayerStatusBar on mouse activity
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private async void OnActivity(object sender, PreProcessInputEventArgs e)
        {
            await MouseActivitySemaphore.WaitAsync();
            if (e.StagingItem == null)
            {
                MouseActivitySemaphore.Release();
                return;
            }

            var inputEventArgs = e.StagingItem.Input;
            if (!(inputEventArgs is MouseEventArgs) && !(inputEventArgs is KeyboardEventArgs))
            {
                MouseActivitySemaphore.Release();
                return;
            }

            var mouseEventArgs = e.StagingItem.Input as MouseEventArgs;

            // no button is pressed and the position is still the same as the application became inactive
            if (mouseEventArgs?.LeftButton == MouseButtonState.Released &&
                mouseEventArgs.RightButton == MouseButtonState.Released &&
                mouseEventArgs.MiddleButton == MouseButtonState.Released &&
                mouseEventArgs.XButton1 == MouseButtonState.Released &&
                mouseEventArgs.XButton2 == MouseButtonState.Released &&
                InactiveMousePosition == mouseEventArgs.GetPosition(Container))
            {
                MouseActivitySemaphore.Release();
                return;
            }

            var opacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                KeyFrames = new DoubleKeyFrameCollection
                {
                    new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0), new PowerEase
                    {
                        EasingMode = EasingMode.EaseInOut
                    })
                }
            };

            PlayerStatusBar.BeginAnimation(OpacityProperty, opacityAnimation);
            UpperPanel.BeginAnimation(OpacityProperty, opacityAnimation);
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Cursor = Cursors.Arrow;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            MouseActivitySemaphore.Release();
        }

        /// <summary>
        /// Handles the FFmpegMessageLogged event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MediaLogMessageEventArgs"/> instance containing the event data.</param>
        private void OnMediaFFmpegMessageLogged(object sender, MediaLogMessageEventArgs e)
        {
            if (e.MessageType == MediaLogMessageType.Error)
            {
                //TODO
            }
        }

        /// <summary>
        /// Dispose the control
        /// </summary>
        private async Task Unload()
        {
            try
            {
                Loaded -= OnLoaded;
                ActivityTimer.Tick -= OnInactivity;
                ActivityTimer.Stop();

                InputManager.Current.PreProcessInput -= OnActivity;
                Media.MediaOpened -= OnMediaOpened;
                Media.MediaOpening -= OnMediaOpening;
                Media.MediaFailed -= EncounteredError;
                Media.MediaEnded -= MediaPlayerEndReached;
                Media.SeekingStarted -= OnSeekingStarted;
                Media.SeekingEnded -= OnSeekingEnded;
                Media.MediaChanging -= OnMediaChanging;
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Cursor = Cursors.Arrow;
                }

                var vm = DataContext as MediaPlayerViewModel;
                if (vm != null)
                {
                    vm.StoppedMedia -= OnStoppedMedia;
                    vm.SubtitleChanged -= OnSubtitleChanged;
                }

                if (vm?.BandwidthRate != null)
                {
                    vm.BandwidthRate.ProgressChanged -= OnBandwidthChanged;
                }

                if (vm?.BufferProgress != null)
                {
                    vm.BufferProgress.ProgressChanged -= OnBufferProgressChanged;
                }

                MediaElement.FFmpegMessageLogged -= OnMediaFFmpegMessageLogged;
                Messenger.Default.Unregister<KeyPressedMessage>(this);
                _applicationService.SwitchConstantDisplayAndPower(false);
                await Media.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}