using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Popcorn.Converters
{
    /// <summary>
    /// Convert double to double using a ratio parameter
    /// </summary>
    [ValueConversion(typeof (string), typeof (string))]
    public class RatioConverter : MarkupExtension, IValueConverter
    {
        private RatioConverter _instance;

        /// <summary>
        /// Modify value with a ratio parameter
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Double value affected by the ratio</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = 0d;
            if (value != null)
                size = System.Convert.ToDouble(value, CultureInfo.InvariantCulture)*
                       System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);

            return size;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => _instance ?? (_instance = new RatioConverter());
    }
}