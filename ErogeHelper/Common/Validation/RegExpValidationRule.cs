using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    class RegExpValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? pattern = value as string;
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return ValidationResult.ValidResult;
            }

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            try
            {
                Regex optionRegex = new Regex(pattern, options);
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
