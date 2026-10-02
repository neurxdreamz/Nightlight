using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nightlight
{
    //конвертер для bool в цвет
    public class BoolToBrushConverter : IValueConverter
    {
        //превращает значение в кисть
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //приводим к bool
            bool flag = (bool)value;

            //зеленый если true, красный если false
            if (flag == true)
            {
                return Brushes.LimeGreen;
            }
            else
            {
                return Brushes.IndianRed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}