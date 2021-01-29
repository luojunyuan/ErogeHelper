using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    class InvalidCodeFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // HCode 0或1个/ H 1个以上任意字符 @ 1个以上十六进制 : 1个以上任意字符
            // RCode 0或1个/ RS@ 1个以上十六进制
            string patten = @"/?H\S+@[A-Fa-f0-9]+:\S+|/?RS@[A-Fa-f0-9]+";

            return string.IsNullOrWhiteSpace((value ?? "").ToString()) || Regex.IsMatch((value ?? "").ToString() ?? "", patten)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid HCode.");
        }
    }
}
