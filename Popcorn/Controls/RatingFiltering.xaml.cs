using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Popcorn.Extensions;

namespace Popcorn.Controls
{
    /// <summary>
    /// Interaction logic for Rating.xaml
    /// </summary>
    public partial class RatingFiltering
    {
        /// <summary>
        /// Max rating value
        /// </summary>
        private const int Max = 10;

        /// <summary>
        /// Rating property
        /// </summary>
        public static readonly DependencyProperty RatingValueProperty = DependencyProperty.Register("RatingValue",
            typeof(double), typeof(RatingFiltering),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RatingChanged));

        /// <summary>
        /// Initialize a new instance of RatingFiltering
        /// </summary>
        public RatingFiltering()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Rating property
        /// </summary>
        public double RatingValue
        {
            get => (double) GetValue(RatingValueProperty);
            set
            {
                if (value < 0)
                    SetValue(RatingValueProperty, 0);
                else if (value > Max)
                    SetValue(RatingValueProperty, Max);
                else
                    SetValue(RatingValueProperty, value);
            }
        }

        /// <summary>
        /// Set IsChecked for each star on rating changed
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private static void RatingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rating = sender as RatingFiltering;
            if (rating == null)
                return;

            var newval = Convert.ToInt32(Math.Ceiling((double) e.NewValue));
            newval /= 2;
            var childs = ((Grid) (rating.Content)).Children;

            ToggleButton button;

            for (var i = 0; i < newval; i++)
            {
                button = childs[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = true;
            }

            for (var i = newval; i < childs.Count; i++)
            {
                button = childs[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = false;
            }
        }

        /// <summary>
        /// Toggle star on click event
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void ToggleStar(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton button))
                return;

            if (button.Name == "StarOne")
            {
                if (!StarOne.IsChecked.HasValue || !StarOne.IsChecked.Value)
                {
                    if (!StarTwo.IsChecked.Value)
                    {
                        RatingValue = 0;
                        StarTwo.IsChecked = false;
                        StarThree.IsChecked = false;
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                    }
                    else
                    {
                        StarTwo.IsChecked = false;
                        StarThree.IsChecked = false;
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 2;
                        OnMouseLeaveStarTwo(null, null);
                        OnMouseLeaveStarThree(null, null);
                        OnMouseLeaveStarFour(null, null);
                        OnMouseLeaveStarFive(null, null);
                    }
                }
                else
                {
                    RatingValue = 2;
                    StarTwo.IsChecked = false;
                    StarThree.IsChecked = false;
                    StarFour.IsChecked = false;
                    StarFive.IsChecked = false;
                }
            }
            else if (button.Name == "StarTwo")
            {
                if (!StarTwo.IsChecked.HasValue || !StarTwo.IsChecked.Value)
                {
                    if (!StarThree.IsChecked.Value)
                    {
                        StarOne.IsChecked = false;
                        StarThree.IsChecked = false;
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 0;
                    }
                    else
                    {
                        StarThree.IsChecked = false;
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 4;
                        OnMouseLeaveStarThree(null, null);
                        OnMouseLeaveStarFour(null, null);
                        OnMouseLeaveStarFive(null, null);
                    }
                }
                else
                {
                    StarOne.IsChecked = true;
                    StarThree.IsChecked = false;
                    StarFour.IsChecked = false;
                    StarFive.IsChecked = false;
                    RatingValue = 4;
                }
            }
            else if (button.Name == "StarThree")
            {
                if (!StarThree.IsChecked.HasValue || !StarThree.IsChecked.Value)
                {
                    if (!StarFour.IsChecked.Value)
                    {
                        StarOne.IsChecked = false;
                        StarTwo.IsChecked = false;
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 0;
                    }
                    else
                    {
                        StarFour.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 6;
                        OnMouseLeaveStarFour(null, null);
                        OnMouseLeaveStarFive(null, null);
                    }
                }
                else
                {
                    StarOne.IsChecked = true;
                    StarTwo.IsChecked = true;
                    StarFour.IsChecked = false;
                    StarFive.IsChecked = false;
                    RatingValue = 6;
                }
            }
            else if (button.Name == "StarFour")
            {
                if (!StarFour.IsChecked.HasValue || !StarFour.IsChecked.Value)
                {
                    if (!StarFive.IsChecked.Value)
                    {
                        StarOne.IsChecked = false;
                        StarTwo.IsChecked = false;
                        StarThree.IsChecked = false;
                        StarFive.IsChecked = false;
                        RatingValue = 0;
                    }
                    else
                    {
                        StarFive.IsChecked = false;
                        RatingValue = 8;
                        OnMouseLeaveStarFive(null, null);
                    }
                }
                else
                {
                    StarOne.IsChecked = true;
                    StarTwo.IsChecked = true;
                    StarThree.IsChecked = true;
                    StarFive.IsChecked = false;
                    RatingValue = 8;
                }
            }
            else if (button.Name == "StarFive")
            {
                if (!StarFive.IsChecked.HasValue || !StarFive.IsChecked.Value)
                {
                    StarOne.IsChecked = false;
                    StarTwo.IsChecked = false;
                    StarThree.IsChecked = false;
                    StarFour.IsChecked = false;
                    RatingValue = 0;
                }
                else
                {
                    StarOne.IsChecked = true;
                    StarTwo.IsChecked = true;
                    StarThree.IsChecked = true;
                    StarFour.IsChecked = true;
                    StarFive.IsChecked = true;
                    RatingValue = 9.1;
                }
            }
        }

        private void OnMouseEnterStarOne(object sender, MouseEventArgs e)
        {
            if (!StarOne.IsChecked.HasValue || !StarOne.IsChecked.Value)
            {
                var star = StarOne.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.DarkOrange;
                path.Opacity = 0.8;
            }
        }

        private void OnMouseLeaveStarOne(object sender, MouseEventArgs e)
        {
            if (!StarOne.IsChecked.HasValue || !StarOne.IsChecked.Value)
            {
                var star = StarOne.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.White;
                path.Opacity = 1.0;
            }
        }

        private void OnMouseEnterStarTwo(object sender, MouseEventArgs e)
        {
            OnMouseEnterStarOne(null, null);

            if (!StarTwo.IsChecked.HasValue || !StarTwo.IsChecked.Value)
            {
                var star = StarTwo.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.DarkOrange;
                path.Opacity = 0.8;
            }
        }

        private void OnMouseLeaveStarTwo(object sender, MouseEventArgs e)
        {
            OnMouseLeaveStarOne(null, null);

            if (!StarTwo.IsChecked.HasValue || !StarTwo.IsChecked.Value)
            {
                var star = StarTwo.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.White;
                path.Opacity = 1.0;
            }
        }

        private void OnMouseEnterStarThree(object sender, MouseEventArgs e)
        {
            OnMouseEnterStarTwo(null, null);

            if (!StarThree.IsChecked.HasValue || !StarThree.IsChecked.Value)
            {
                var star = StarThree.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.DarkOrange;
                path.Opacity = 0.8;
            }
        }

        private void OnMouseLeaveStarThree(object sender, MouseEventArgs e)
        {
            OnMouseLeaveStarTwo(null, null);

            if (!StarThree.IsChecked.HasValue || !StarThree.IsChecked.Value)
            {
                var star = StarThree.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.White;
                path.Opacity = 1.0;
            }
        }

        private void OnMouseEnterStarFour(object sender, MouseEventArgs e)
        {
            OnMouseEnterStarThree(null, null);

            if (!StarFour.IsChecked.HasValue || !StarFour.IsChecked.Value)
            {
                var star = StarFour.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.DarkOrange;
                path.Opacity = 0.8;
            }
        }

        private void OnMouseLeaveStarFour(object sender, MouseEventArgs e)
        {
            OnMouseLeaveStarThree(null, null);

            if (!StarFour.IsChecked.HasValue || !StarFour.IsChecked.Value)
            {
                var star = StarFour.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.White;
                path.Opacity = 1.0;
            }
        }

        private void OnMouseEnterStarFive(object sender, MouseEventArgs e)
        {
            OnMouseEnterStarFour(null, null);

            if (!StarFive.IsChecked.HasValue || !StarFive.IsChecked.Value)
            {
                var star = StarFive.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.DarkOrange;
                path.Opacity = 0.8;
            }
        }

        private void OnMouseLeaveStarFive(object sender, MouseEventArgs e)
        {
            OnMouseLeaveStarFour(null, null);

            if (!StarFive.IsChecked.HasValue || !StarFive.IsChecked.Value)
            {
                var star = StarFive.FindChild<Rectangle>("star");
                var brush = star.OpacityMask as VisualBrush;
                var visual = brush?.Visual as Canvas;
                var path = visual?.Children[0] as Path;
                path.Fill = Brushes.White;
                path.Opacity = 1.0;
            }
        }
    }
}