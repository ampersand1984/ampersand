using ampersand.Core.Common;
using System;
using System.Windows;
using System.Windows.Data;

namespace ampersand_pb.Common
{
    public class EqualToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var strValue = value != null ? value.ToString() : string.Empty;
            var strParameter = parameter != null ? parameter.ToString() : string.Empty;

            if (strValue.IsNullOrEmpty() || strParameter.IsNullOrEmpty())
                return Visibility.Collapsed;

            var result = strValue.Equals(strParameter) ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
