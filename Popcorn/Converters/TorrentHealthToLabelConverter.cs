using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Popcorn.Helpers;

namespace Popcorn.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class TorrentHealthToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number = (double)value;
            double min = 0;
            double max = 100;

            // Get the value limits from parameter
            try
            {
                string[] limits = (parameter as string).Split(new char[] { '|' });
                min = double.Parse(limits[0], CultureInfo.InvariantCulture);
                max = double.Parse(limits[1], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ArgumentException("Parameter not valid. Enter in format: 'MinDouble|MaxDouble'");
            }

            if (max <= min)
            {
                throw new ArgumentException("Parameter not valid. MaxDouble has to be greater then MinDouble.");
            }

            if (number >= min && number <= max)
            {
                if (number > 8)
                {
                    return LocalizationProviderHelper.GetLocalizedValue<string>("VeryGoodLabel");
                }

                if (number > 6)
                {
                    return LocalizationProviderHelper.GetLocalizedValue<string>("GoodLabel");
                }

                if (number > 4)
                {
                    return LocalizationProviderHelper.GetLocalizedValue<string>("AverageLabel");
                }

                if (number > 2)
                {
                    return LocalizationProviderHelper.GetLocalizedValue<string>("BadLabel");
                }

                if (number >= 0)
                {
                    return LocalizationProviderHelper.GetLocalizedValue<string>("VeryBadLabel");
                }
            }

            return LocalizationProviderHelper.GetLocalizedValue<string>("UnknownLabel");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
