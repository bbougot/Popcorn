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

namespace Popcorn.UserControls.Home.Settings.Help
{
    /// <summary>
    /// Logique d'interaction pour HelpUserControl.xaml
    /// </summary>
    public partial class HelpUserControl : UserControl
    {
        public HelpUserControl()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheelScroller(object sender, MouseWheelEventArgs e)
        {
            var scv = (AnimatedScrollViewer)sender;
            if (scv.TargetHorizontalOffset - e.Delta >= -Math.Abs(e.Delta) &&
                scv.TargetHorizontalOffset - e.Delta < scv.ScrollableWidth + Math.Abs(e.Delta))
            {
                scv.TargetHorizontalOffset -= e.Delta;
            }
        }
    }
}
