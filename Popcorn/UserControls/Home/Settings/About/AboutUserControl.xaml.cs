using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Controls;
using Popcorn.Messaging;
using Popcorn.Exceptions;

namespace Popcorn.UserControls.Home.Settings.About
{
    /// <summary>
    /// Logique d'interaction pour AboutUserControl.xaml
    /// </summary>
    public partial class AboutUserControl : UserControl
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public AboutUserControl()
        {
            InitializeComponent();
        }

        private void CanGoToPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PerformGoToPage(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Parameter.ToString()));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Messenger.Default.Send(
                    new UnhandledExceptionMessage(
                        new PopcornException(ex.Message)));
            }
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            if (scv.HorizontalOffset - e.Delta >= -Math.Abs(e.Delta) &&
                scv.HorizontalOffset - e.Delta < scv.ScrollableWidth + Math.Abs(e.Delta))
            {
                var element = Mouse.DirectlyOver;
                if (!(element is ScrollViewer))
                {
                    e.Handled = false;
                    return;
                }

                scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            }
        }
    }
}
