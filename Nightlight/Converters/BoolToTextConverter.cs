using System;
using System.Globalization;
using System.Windows.Data;

namespace Nightlight
{
    //конвертер для bool в текст
    public class BoolToTextConverter : IValueConverter
    {
        //превращает значение в текст
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //приводим к bool
            bool flag = (bool)value;

            if (flag == true)
            {
                return "выполнено";
            }
            else
            {
                return "не выполнено";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}