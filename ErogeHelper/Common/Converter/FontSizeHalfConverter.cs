using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace ErogeHelper.Common.Converter
{
    // 实现MarkupExtension可以在xaml中直接引用convert，否则需要定义StaticResource
    public class FontSizeHalfConverter : MarkupExtension, IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2 + 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static FontSizeHalfConverter? _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new FontSizeHalfConverter();
            return _converter;
        }
    }
}
