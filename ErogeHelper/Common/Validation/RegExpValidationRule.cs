using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    public class RegExpValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var pattern = value as string;
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return ValidationResult.ValidResult;
            }

            var options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            try
            {
                var optionRegex = new Regex(pattern, options);
            }
            catch (ArgumentException ex)
            {
                Log.Debug("Check RegExp format, it's fine exception");
                return new ValidationResult(false, $"Invalid RegExp. {ex.Message}");
            }

            return ValidationResult.ValidResult;
        }
    }
}