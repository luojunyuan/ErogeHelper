using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    public class FalseToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
            {
                return Visibility.Collapsed;
            }
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility)
            {
                return Visibility.Visible;
            }
            return (Visibility)value != Visibility.Visible;
        }
    }
}