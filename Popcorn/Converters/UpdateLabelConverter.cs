using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Popcorn.Helpers;

namespace Popcorn.Converters
{
    public class UpdateLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return LocalizationProviderHelper.GetLocalizedValue<string>(parameter.ToString())
                .Replace("%PERCENTAGE%", $"{value.ToString()}%");
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}