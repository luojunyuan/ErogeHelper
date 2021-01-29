using System;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    class HookEngineHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString()!.Contains("UserHook"))
            {
                return "UserHook";
            }
            else if (value.ToString()!.Equals("READ"))
            {
                return "READ";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
