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
            string patten = @"\S+@[A-Fa-f0-9]+:\S+";

            return string.IsNullOrWhiteSpace((value ?? "").ToString()) || Regex.IsMatch((value ?? "").ToString() ?? "", patten)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid HCode.");
        }
    }
}
