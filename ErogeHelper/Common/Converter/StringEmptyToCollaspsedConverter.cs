using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    class StringEmptyToCollaspsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (text is not null)
            {
                return text.Equals(string.Empty) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }    
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
