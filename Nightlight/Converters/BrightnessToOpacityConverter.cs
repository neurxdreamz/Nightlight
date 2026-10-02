using System;
using System.Globalization;
using System.Windows.Data;

namespace Nightlight
{
    //конвертер яркости в прозрачность свечения лампы
    public class BrightnessToOpacityConverter : IValueConverter
    {
        //превращает int в double
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //приводим к int
            int brightness = (int)value;

            //защита от отрицательных значений
            if (brightness < 0)
            {
                brightness = 0;
            }

            //защита от значений больше 100
            if (brightness > 100)
            {
                brightness = 100;
            }

            //яркость 0-100 превращаем в прозрачность 0-1
            double opacity = brightness * 0.01;

            return opacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}