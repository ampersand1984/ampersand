using System;
using System.Windows;
using System.Windows.Data;

namespace ampersand_pb.Common
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = parameter == null ?
                (value != null ? Visibility.Visible : Visibility.Collapsed) ://normal
                (value != null ? Visibility.Collapsed : Visibility.Visible); //negate

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
