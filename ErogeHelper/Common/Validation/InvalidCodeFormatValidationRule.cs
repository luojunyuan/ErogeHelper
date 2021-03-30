using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    public class InvalidCodeFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // HCode 0或1个/ H 1个以上任意字符 @ 1个以上十六进制 : 1个以上任意字符
            // RCode 0或1个/ RS@ 1个以上十六进制
            const string patten = @"/?H\S+@[A-Fa-f0-9]+:\S+|/?RS@[A-Fa-f0-9]+";

            var code = value as string;

            if (string.IsNullOrWhiteSpace(code))
            {
                // if hcode is null or space, make TextBox normal style
                return ValidationResult.ValidResult;
            }

            return Regex.IsMatch(code, patten)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid HCode.");
        }
    }
}