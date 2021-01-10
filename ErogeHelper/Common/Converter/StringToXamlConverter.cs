using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace ErogeHelper.Common.Converter
{
    class StringToXamlConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? input = value as string;
            if (input is not null)
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap
                };
                string? escapedXml = SecurityElement.Escape(input);

                while (escapedXml?.IndexOf("|~S~|") != -1)
                {
                    //up to |~S~| is normal
                    textBlock.Inlines.Add(new Run(escapedXml?.Substring(0, escapedXml.IndexOf("|~S~|"))));
                    //between |~S~| and |~E~| is highlighted
                    textBlock.Inlines.Add(new Run(escapedXml?.Substring(escapedXml.IndexOf("|~S~|") + 5,
                                              escapedXml.IndexOf("|~E~|") - (escapedXml.IndexOf("|~S~|") + 5)))
                    { TextDecorations = TextDecorations.Strikethrough });
                    //the rest of the string (after the |~E~|)
                    escapedXml = escapedXml?.Substring(escapedXml.IndexOf("|~E~|") + 5);
                }

                if (escapedXml.Length > 0)
                {
                    textBlock.Inlines.Add(new Run(escapedXml));
                }
                return textBlock;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This converter cannot be used in two-way binding.");
        }
    }
}
