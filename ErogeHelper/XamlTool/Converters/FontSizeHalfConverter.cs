using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ErogeHelper.XamlTool.Converters;

public class FontSizeHalfConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (double)value / 2 + 5;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }

    private static FontSizeHalfConverter? _converter;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter ??= new FontSizeHalfConverter();
    }
}
