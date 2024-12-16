using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using Popcorn.Controls;
using Popcorn.Converters;
using Popcorn.Helpers;

namespace Popcorn.UserControls.Home.Movie.Details
{
    /// <summary>
    /// Interaction logic for Movie.xaml
    /// </summary>
    public partial class MovieDetailsUserControl
    {
        /// <summary>
        /// Initializes a new instance of the Movie class.
        /// </summary>
        public MovieDetailsUserControl()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scv = (AnimatedScrollViewer) sender;
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

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = PackIconMaterialKind.AccountCircle;
            var converter = new PackIconMaterialImageSourceConverter();
            var bitmapImage = converter.Convert(image, typeof(Image), new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                CultureInfo.InvariantCulture);
            ((Image) sender).Source = (DrawingImage)bitmapImage;
        }
    }
}