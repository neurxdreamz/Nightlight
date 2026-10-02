using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nightlight
{
    //конвертер состояния ночника в цвет ядра лампы
    public class IsOnToGlowBrushConverter : IValueConverter
    {
        //превращает bool в кисть
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //приводим к bool
            bool flag = (bool)value;

            //желтый цвет если ночник включен
            if (flag == true)
            {
                return new SolidColorBrush(Color.FromRgb(255, 210, 100));
            }
            else
            {
                //серый цвет если выключен
                return new SolidColorBrush(Color.FromRgb(80, 80, 90));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}