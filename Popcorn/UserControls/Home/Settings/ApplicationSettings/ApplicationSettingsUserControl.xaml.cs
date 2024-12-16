using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Popcorn.Controls;

namespace Popcorn.UserControls.Home.Settings.ApplicationSettings
{
    /// <summary>
    /// Logique d'interaction pour ApplicationSettingsUserControl.xaml
    /// </summary>
    public partial class ApplicationSettingsUserControl
    {
        public ApplicationSettingsUserControl()
        {
            InitializeComponent();
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
