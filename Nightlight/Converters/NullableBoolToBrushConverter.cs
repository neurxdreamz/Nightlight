using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nightlight
{
    public class NullableBoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Brushes.LightGray;
            }

            bool flag = (bool)value;

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