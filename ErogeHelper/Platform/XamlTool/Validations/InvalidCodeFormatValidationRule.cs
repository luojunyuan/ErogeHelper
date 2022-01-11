using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using ErogeHelper.Shared.Contracts;

namespace ErogeHelper.Platform.XamlTool.Validations;

internal class InvalidCodeFormatValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // HCode 0或1个/ H 1个以上任意字符 @ 1个以上十六进制 (: 1个以上任意字符)
        // RCode 0或1个/ RS@ 1个以上十六进制
        const string patten = ConstantValue.CodeRegExp;

        var code = value as string;

        if (string.IsNullOrWhiteSpace(code))
            // if hcode is null or space, make TextBox normal style
            return ValidationResult.ValidResult;

        if (code[^1] == ':')
            return new ValidationResult(false, "Invalid HCode.");

        return Regex.IsMatch(code, patten)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Invalid HCode.");
    }
}
