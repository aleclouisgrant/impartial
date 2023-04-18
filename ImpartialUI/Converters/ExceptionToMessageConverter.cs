using System;
using System.Globalization;
using System.Windows.Data;

namespace ImpartialUI.Converters
{
    public class ExceptionToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (object.Equals(value, null))
                return "";
            else
                return ((Exception)value).Message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
