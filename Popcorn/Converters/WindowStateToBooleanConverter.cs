using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Popcorn.Converters
{
    /// <summary>
    /// Used to convert a window state to a boolean
    /// </summary>
    [ValueConversion(typeof (WindowState), typeof (bool))]
    public class WindowStateToBooleanConverter : MarkupExtension, IValueConverter
    {
        private WindowStateToBooleanConverter _instance;

        /// <summary>
        /// Convert boolean to a window state
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if maximized, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            var isFullscreen = (bool) value;
            return isFullscreen ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if ((WindowState) value == WindowState.Minimized) return null;
            return (WindowState)value == WindowState.Maximized;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ?? (_instance = new WindowStateToBooleanConverter());
    }
}