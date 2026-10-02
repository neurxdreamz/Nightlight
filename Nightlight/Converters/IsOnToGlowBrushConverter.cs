using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nightlight
{
    public class IsOnToGlowBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)value;

            if (flag == true)
            {
                return new SolidColorBrush(Color.FromRgb(255, 210, 100));
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(80, 80, 90));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}