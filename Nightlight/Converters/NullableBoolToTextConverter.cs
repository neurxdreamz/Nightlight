using System;
using System.Globalization;
using System.Windows.Data;

namespace Nightlight
{
    public class NullableBoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "не выполнялось";
            }

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