using System;
using System.Globalization;
using System.Windows.Data;
using ImpartialUI.Enums;

namespace ImpartialUI.Converters
{
    internal class CompetitionTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((CompetitionType)value)
            {
                case CompetitionType.JackAndJill:
                    return "Jack & Jill";
                case CompetitionType.Strictly:
                    return "Strictly";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
