using ErogeHelper.Model.Translator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    class LanguageEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Languages lang = (Languages)value;

            return lang switch
            {
                Languages.Auto => "Auto",
                Languages.简体中文 => "简体中文",
                Languages.English => "English",
                Languages.日本語 => "日本語",
                _ => throw new Exception("Error language")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringText = value as string;
            return stringText switch
            {
                "简体中文" => Languages.简体中文,
                "English" => Languages.English,
                "日本語" => Languages.日本語,
                _ => Languages.Auto
            };
        }
    }
}
