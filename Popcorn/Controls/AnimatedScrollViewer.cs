using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using NLog;

namespace Popcorn.Controls
{
    [TemplatePart(Name = "PART_AniVerticalScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_AniHorizontalScrollBar", Type = typeof(ScrollBar))]

    public class AnimatedScrollViewer : ScrollViewer
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        #region PART items

        ScrollBar _aniVerticalScrollBar;
        ScrollBar _aniHorizontalScrollBar;

        #endregion

        static AnimatedScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedScrollViewer),
                new FrameworkPropertyMetadata(typeof(AnimatedScrollViewer)));
        }

        #region ScrollViewer Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ScrollBar aniVScroll = GetTemplateChild("PART_AniVerticalScrollBar") as ScrollBar;
            if (aniVScroll != null)
            {
                _aniVerticalScrollBar = aniVScroll;
            }

            _aniVerticalScrollBar.ValueChanged += VScrollBar_ValueChanged;

            ScrollBar aniHScroll = GetTemplateChild("PART_AniHorizontalScrollBar") as ScrollBar;
            if (aniHScroll != null)
            {
                _aniHorizontalScrollBar = aniHScroll;
            }

            _aniHorizontalScrollBar.ValueChanged += HScrollBar_ValueChanged;

            PreviewMouseWheel += CustomPreviewMouseWheel;
            PreviewKeyDown += AnimatedScrollViewer_PreviewKeyDown;
        }

        void AnimatedScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer) sender;

            if (thisScroller.CanKeyboardScroll)
            {
                Key keyPressed = e.Key;
                double newVerticalPos = thisScroller.TargetVerticalOffset;
                double newHorizontalPos = thisScroller.TargetHorizontalOffset;
                bool isKeyHandled = false;

                //Vertical Key Strokes code
                if (keyPressed == Key.Down)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos + 16.0), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageDown)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos + thisScroller.ViewportHeight),
                        Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.Up)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos - 16.0), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageUp)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos - thisScroller.ViewportHeight),
                        Orientation.Vertical);
                    isKeyHandled = true;
                }

                if (!newVerticalPos.Equals(thisScroller.TargetVerticalOffset))
                {
                    thisScroller.TargetVerticalOffset = newVerticalPos;
                }

                //Horizontal Key Strokes Code

                if (keyPressed == Key.Right)
                {
                    newHorizontalPos = NormalizeScrollPos(thisScroller, (newHorizontalPos + 16),
                        Orientation.Horizontal);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.Left)
                {
                    newHorizontalPos = NormalizeScrollPos(thisScroller, (newHorizontalPos - 16),
                        Orientation.Horizontal);
                    isKeyHandled = true;
                }

                if (!newHorizontalPos.Equals(thisScroller.TargetHorizontalOffset))
                {
                    thisScroller.TargetHorizontalOffset = newHorizontalPos;
                }

                e.Handled = isKeyHandled;
            }
        }

        private double NormalizeScrollPos(AnimatedScrollViewer thisScroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > thisScroll.ScrollableHeight)
            {
                returnValue = thisScroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > thisScroll.ScrollableWidth)
            {
                returnValue = thisScroll.ScrollableWidth;
            }

            return returnValue;
        }

        #endregion

        #region Custom Event Handlers

        void CustomPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouseWheelChange = e.Delta;

            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer) sender;
            double newVOffset = thisScroller.TargetVerticalOffset - (mouseWheelChange / 3);
            if (newVOffset < 0)
            {
                thisScroller.TargetVerticalOffset = 0;
            }
            else if (newVOffset > thisScroller.ScrollableHeight)
            {
                thisScroller.TargetVerticalOffset = thisScroller.ScrollableHeight;
            }
            else
            {
                thisScroller.TargetVerticalOffset = newVOffset;
            }

            e.Handled = true;
        }

        void VScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var thisScroller = this;
            var oldTargetVOffset = e.OldValue;
            var newTargetVOffset = e.NewValue;

            if (!newTargetVOffset.Equals(thisScroller.TargetVerticalOffset))
            {
                var deltaVOffset = Math.Round((newTargetVOffset - oldTargetVOffset), 3);

                if (deltaVOffset.Equals(1d))
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset + thisScroller.ViewportHeight;

                }
                else if (deltaVOffset.Equals(-1d))
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset - thisScroller.ViewportHeight;
                }
                else if (deltaVOffset.Equals(0.1d))
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset + 16.0;
                }
                else if (deltaVOffset.Equals(-0.1d))
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset - 16.0;
                }
                else
                {
                    thisScroller.TargetVerticalOffset = newTargetVOffset;
                }
            }
        }

        void HScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var thisScroller = this;

            var oldTargetHOffset = e.OldValue;
            var newTargetHOffset = e.NewValue;

            if (!newTargetHOffset.Equals(thisScroller.TargetHorizontalOffset))
            {

                var deltaVOffset = Math.Round((newTargetHOffset - oldTargetHOffset), 3);

                if (deltaVOffset.Equals(1d))
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset + thisScroller.ViewportWidth;

                }
                else if (deltaVOffset.Equals(-1d))
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset - thisScroller.ViewportWidth;
                }
                else if (deltaVOffset.Equals(0.1d))
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset + 16.0;
                }
                else if (deltaVOffset.Equals(-0.1d))
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset - 16.0;
                }
                else
                {
                    thisScroller.TargetHorizontalOffset = newTargetHOffset;
                }
            }
        }

        #endregion

        #region Custom Dependency Properties

        #region TargetVerticalOffset (DependencyProperty)(double)

        /// <summary>
        /// This is the VerticalOffset that we'd like to animate to
        /// </summary>
        public double TargetVerticalOffset
        {
            get => (double) GetValue(TargetVerticalOffsetProperty);
            set => SetValue(TargetVerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty TargetVerticalOffsetProperty =
            DependencyProperty.Register("TargetVerticalOffset", typeof(double), typeof(AnimatedScrollViewer),
                new PropertyMetadata(0.0, OnTargetVerticalOffsetChanged));

        private static void OnTargetVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var thisScroller = (AnimatedScrollViewer) d;

                if (!((double) e.NewValue).Equals(thisScroller._aniVerticalScrollBar.Value))
                {
                    thisScroller._aniVerticalScrollBar.Value = (double) e.NewValue;
                }

                thisScroller.animateScroller(thisScroller);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region TargetHorizontalOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the HorizontalOffset that we'll be animating to
        /// </summary>
        public double TargetHorizontalOffset
        {
            get => (double) GetValue(TargetHorizontalOffsetProperty);
            set => SetValue(TargetHorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty TargetHorizontalOffsetProperty =
            DependencyProperty.Register("TargetHorizontalOffset", typeof(double), typeof(AnimatedScrollViewer),
                new PropertyMetadata(0.0, OnTargetHorizontalOffsetChanged));

        private static void OnTargetHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer) d;

            if (!((double) e.NewValue).Equals(thisScroller._aniHorizontalScrollBar.Value))
            {
                thisScroller._aniHorizontalScrollBar.Value = (double) e.NewValue;
            }

            thisScroller.animateScroller(thisScroller);
        }

        #endregion

        #region HorizontalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual horizontal offset property we're going use as an animation helper
        /// </summary>
        public double HorizontalScrollOffset
        {
            get => (double) GetValue(HorizontalScrollOffsetProperty);
            set => SetValue(HorizontalScrollOffsetProperty, value);
        }

        public static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
                new PropertyMetadata(0.0, OnHorizontalScrollOffsetChanged));

        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer) d;
            thisSViewer.ScrollToHorizontalOffset((double) e.NewValue);
        }

        #endregion

        #region VerticalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual VerticalOffset we're going to use as an animation helper
        /// </summary>
        public double VerticalScrollOffset
        {
            get => (double) GetValue(VerticalScrollOffsetProperty);
            set => SetValue(VerticalScrollOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
                new PropertyMetadata(0.0, OnVerticalScrollOffsetChanged));

        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer) d;
            thisSViewer.ScrollToVerticalOffset((double) e.NewValue);
        }

        #endregion

        #region AnimationTime (DependencyProperty) (TimeSpan)

        /// <summary>
        /// A property for changing the time it takes to scroll to a new 
        ///     position. 
        /// </summary>
        public TimeSpan ScrollingTime
        {
            get => (TimeSpan) GetValue(ScrollingTimeProperty);
            set => SetValue(ScrollingTimeProperty, value);
        }

        public static readonly DependencyProperty ScrollingTimeProperty =
            DependencyProperty.Register("ScrollingTime", typeof(TimeSpan), typeof(AnimatedScrollViewer),
                new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 500)));

        #endregion

        #region ScrollingSpline (DependencyProperty)

        /// <summary>
        /// A property to allow users to describe a custom spline for the scrolling
        ///     animation.
        /// </summary>
        public KeySpline ScrollingSpline
        {
            get => (KeySpline) GetValue(ScrollingSplineProperty);
            set => SetValue(ScrollingSplineProperty, value);
        }

        public static readonly DependencyProperty ScrollingSplineProperty =
            DependencyProperty.Register("ScrollingSpline", typeof(KeySpline), typeof(AnimatedScrollViewer),
                new PropertyMetadata(new KeySpline(0.024, 0.914, 0.717, 1)));

        #endregion

        #region CanKeyboardScroll (Dependency Property)

        public static readonly DependencyProperty CanKeyboardScrollProperty =
            DependencyProperty.Register("CanKeyboardScroll", typeof(bool), typeof(AnimatedScrollViewer),
                new FrameworkPropertyMetadata(true));

        public bool CanKeyboardScroll
        {
            get => (bool) GetValue(CanKeyboardScrollProperty);
            set => SetValue(CanKeyboardScrollProperty, value);
        }

        #endregion

        #endregion

        #region animateScroller method (Creates and runs animation)

        private void animateScroller(object objectToScroll)
        {
            AnimatedScrollViewer thisScrollViewer = objectToScroll as AnimatedScrollViewer;

            KeyTime targetKeyTime = thisScrollViewer.ScrollingTime;
            KeySpline targetKeySpline = thisScrollViewer.ScrollingSpline;

            DoubleAnimationUsingKeyFrames animateHScrollKeyFramed = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames animateVScrollKeyFramed = new DoubleAnimationUsingKeyFrames();

            SplineDoubleKeyFrame hScrollKey1 = new SplineDoubleKeyFrame(thisScrollViewer.TargetHorizontalOffset,
                targetKeyTime, targetKeySpline);
            SplineDoubleKeyFrame vScrollKey1 = new SplineDoubleKeyFrame(thisScrollViewer.TargetVerticalOffset,
                targetKeyTime, targetKeySpline);
            animateHScrollKeyFramed.KeyFrames.Add(hScrollKey1);
            animateVScrollKeyFramed.KeyFrames.Add(vScrollKey1);

            thisScrollViewer.BeginAnimation(HorizontalScrollOffsetProperty, animateHScrollKeyFramed);
            thisScrollViewer.BeginAnimation(VerticalScrollOffsetProperty, animateVScrollKeyFramed);
        }

        #endregion
    }
}