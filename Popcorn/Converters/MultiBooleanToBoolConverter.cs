using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Popcorn.Converters
{
    /// <summary>
    /// Convert booleans to an OR boolean
    /// </summary>
    public class MultiBooleanToBoolConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert booleans to an OR boolean
        /// </summary>
        /// <param name="values">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if all booleans are true, false otherwise</returns>
        public object Convert(object[] values, Type targetType, object parameter,
            CultureInfo culture) => values.OfType<bool>().Aggregate(false, (current, value) => current || value);

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetTypes">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}