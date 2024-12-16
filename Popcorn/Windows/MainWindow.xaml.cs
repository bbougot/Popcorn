using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Controls;
using Popcorn.Extensions;
using Popcorn.Messaging;
using Popcorn.Utils;
using Popcorn.ViewModels.Windows;

namespace Popcorn.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private LowLevelKeyboardListener _listener;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.GotFocusEvent,
                new RoutedEventHandler(RemoveFocusVisualStyle), true);
            Initialized += OnInitialized;
            InitializeComponent();
            Application.Current.Exit += OnExit;
            var vm = DataContext as WindowViewModel;
            if (vm != null)
            {
                vm.NavigationService = MainFrame.NavigationService;
            }

            Messenger.Default.Register<DropFileMessage>(this, e =>
            {
                if (e.Event == DropFileMessage.DropFileEvent.Enter)
                {
                    BorderThickness = new Thickness(1);
                    BorderBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#CCE51400");
                    GlowBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#CCE51400");
                    DoubleAnimation da = new DoubleAnimation
                    {
                        To = 0.5d,
                        Duration = new Duration(TimeSpan.FromMilliseconds(750)),
                        EasingFunction = new PowerEase
                        {
                            EasingMode = EasingMode.EaseInOut,
                            Power = 2d
                        }
                    };
                    BeginAnimation(OpacityProperty, da);
                }
                else
                {
                    BorderThickness = new Thickness(0);
                    BorderBrush = Brushes.Transparent;
                    GlowBrush = Brushes.Transparent;
                    DoubleAnimation da = new DoubleAnimation
                    {
                        To = 1.0d,
                        Duration = new Duration(TimeSpan.FromMilliseconds(750)),
                        EasingFunction = new PowerEase
                        {
                            EasingMode = EasingMode.EaseInOut,
                            Power = 2d
                        }
                    };
                    BeginAnimation(OpacityProperty, da);
                }
            });
        }

        private void RemoveFocusVisualStyle(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).FocusVisualStyle = null;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += OnKeyPressed;
            _listener.HookKeyboard();
        }

        private void OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (IsActive)
            {
                Messenger.Default.Send(new KeyPressedMessage(e));
                if (e.KeyPressed == Key.Down || e.KeyPressed == Key.Up)
                {
                    var movieScrollviewer =
                        this.FindChild<AnimatedScrollViewer>("MovieScrollViewer");
                    var showScrollviewer =
                        this.FindChild<AnimatedScrollViewer>("ShowScrollViewer");
                    if (movieScrollviewer != null && movieScrollviewer.IsVisible)
                        movieScrollviewer.Focus();

                    if (showScrollviewer != null && showScrollviewer.IsVisible)
                        showScrollviewer.Focus();
                }

                if (e.KeyPressed == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                    Clipboard.ContainsText())
                {
                    var clipboard = Clipboard.GetText();
                    if (clipboard.StartsWith("magnet"))
                    {
                        Messenger.Default.Send(new DownloadMagnetLinkMessage(clipboard));
                    }
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var searchBox =
                this.FindChild<TextBox>("SearchBox");
            if (e.Key == Key.F3 || (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift &&
                e.Key == Key.F)
            {
                searchBox?.Focus();
            }
        }

        private void OnActivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window != null)
            {
                window.Activated -= OnActivated;
                window.Topmost = false;
                window.Focus();
            }
        }
    }
}