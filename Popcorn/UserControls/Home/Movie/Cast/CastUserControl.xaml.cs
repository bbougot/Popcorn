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
using Popcorn.Helpers;

namespace Popcorn.UserControls.Home.Movie.Cast
{
    /// <summary>
    /// Logique d'interaction pour CastUserControl.xaml
    /// </summary>
    public partial class CastUserControl : UserControl
    {
        public CastUserControl()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scv = (AnimatedScrollViewer)sender;
            if (scv.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                scv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                return;
            }

            scv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            if (scv.TargetHorizontalOffset - e.Delta >= -Math.Abs(e.Delta) &&
                scv.TargetHorizontalOffset - e.Delta < scv.ScrollableWidth + Math.Abs(e.Delta))
            {
                scv.TargetHorizontalOffset -= e.Delta;
            }
        }
    }
}
