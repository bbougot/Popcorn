using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Popcorn.Controls
{
    public class Windows10ToggleSwitch : CheckBox
    {
        public Brush Accent
        {
            get => (Brush) GetValue(AccentProperty);
            set => SetValue(AccentProperty, value);
        }

        public static readonly DependencyProperty AccentProperty =
            DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(Windows10ToggleSwitch),
                new PropertyMetadata(new SolidColorBrush(Colors.Blue)));

        public string OnLabel
        {
            get => Convert.ToString(GetValue(OnLabelProperty));
            set => SetValue(OnLabelProperty, value);
        }

        public static readonly DependencyProperty OnLabelProperty =
            DependencyProperty.Register(nameof(OnLabel), typeof(string), typeof(Windows10ToggleSwitch),
                new PropertyMetadata("On"));

        public string OffLabel
        {
            get => Convert.ToString(GetValue(OffLabelProperty));
            set => SetValue(OffLabelProperty, value);
        }

        public static readonly DependencyProperty OffLabelProperty =
            DependencyProperty.Register(nameof(OffLabel), typeof(string), typeof(Windows10ToggleSwitch),
                new PropertyMetadata("Off"));

        static Windows10ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Windows10ToggleSwitch),
                new FrameworkPropertyMetadata(typeof(Windows10ToggleSwitch)));
        }
    }
}
