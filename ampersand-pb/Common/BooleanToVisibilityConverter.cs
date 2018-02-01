using System;
using System.Windows;
using System.Windows.Data;

namespace ampersand_pb.Common
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private object GetVisibility(object value, object parameter)
        {
            if (!(value is bool))
                return Visibility.Collapsed;

            var objValue = (bool)value;
            if (objValue)
            {
                return parameter == null ? Visibility.Visible : Visibility.Collapsed;
            }
            return parameter == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetVisibility(value, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
