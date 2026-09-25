using System;
using System.Globalization;
using System.Windows.Data;

namespace Nightlight
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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