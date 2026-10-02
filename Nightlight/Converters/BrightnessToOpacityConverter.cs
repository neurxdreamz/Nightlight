using System;
using System.Globalization;
using System.Windows.Data;

namespace Nightlight
{
    public class BrightnessToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int brightness = (int)value;

            if (brightness < 0)
            {
                brightness = 0;
            }

            if (brightness > 100)
            {
                brightness = 100;
            }

            double opacity = brightness * 0.01;

            return opacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}