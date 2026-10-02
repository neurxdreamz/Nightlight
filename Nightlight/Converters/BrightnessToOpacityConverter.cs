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

            //0 -> 0.15(едва светит),100 -> 1.0(ярко светит)
            double opacity = 0.15 + (brightness * 0.0085);

            return opacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}