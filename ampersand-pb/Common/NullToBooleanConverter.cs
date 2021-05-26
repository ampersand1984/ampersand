using System;
using System.Globalization;
using System.Windows.Data;

namespace ampersand_pb.Common
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter == null ?
                value != null :
                value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }
    }
}
