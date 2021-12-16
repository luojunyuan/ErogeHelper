using System.Globalization;
using System.Windows.Data;

namespace ErogeHelper.XamlTool.Converters;

public class IsLessThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return false;

        if (!int.TryParse(parameter.ToString(), out var threshold))
            throw new InvalidOperationException("The parameter could not be converted to an integer");

        if (value is double d)
            return d < threshold;
        else if (value is float f)
        {
            return f < threshold;
        }
        else if (value is ulong ul)
        {
            return ul < (ulong)threshold;
        }
        else if (value is long l)
        {
            return l < threshold;
        }
        else if (value is uint ui)
        {
            return ui < threshold;
        }
        else if (value is int i)
        {
            return i < threshold;
        }
        else if (value is ushort us)
        {
            return us < threshold;
        }
        else if (value is short s)
        {
            return s < threshold;
        }
        else
        {
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
