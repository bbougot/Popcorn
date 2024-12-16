using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Popcorn.Behaviors
{
    public class ShowTickValueBehavior : Behavior<Slider>
    {
        private Track _track;

        public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(
            "Prefix",
            typeof(string),
            typeof(ShowTickValueBehavior),
            new PropertyMetadata(default(string)));

        public string Prefix
        {
            get => (string) GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            _track.MouseMove -= TrackOnMouseMove;
            _track = null;
            base.OnDetaching();
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            _track = (Track) AssociatedObject.Template.FindName("PART_Track", AssociatedObject);
            _track.MouseMove += TrackOnMouseMove;
        }

        private void TrackOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var position = mouseEventArgs.GetPosition(_track);
            var valueFromPoint = _track.ValueFromPoint(position);
            var floorOfValueFromPoint = (int) Math.Floor(valueFromPoint);
            var time = TimeSpan.FromMilliseconds(floorOfValueFromPoint);
            var toolTip = string.Format(CultureInfo.InvariantCulture, "{0}{1}", Prefix, time.ToString(@"hh\:mm\:ss"));

            ToolTipService.SetToolTip(_track, toolTip);
        }
    }
}