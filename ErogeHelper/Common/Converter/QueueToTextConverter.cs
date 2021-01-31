using ErogeHelper.Common.Extension;
using System;
using System.Text;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    class QueueToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not ConcurrentCircularBuffer<string> textQueue)
                throw new ArgumentNullException(nameof(value));

            StringBuilder sb = new StringBuilder();
            foreach (var item in textQueue)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("This converter cannot be used in two-way binding.");
            //string[] lines = ((string)value).Split(new string[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //return lines.ToList();
        }
    }
}
