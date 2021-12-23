using System.Reactive;
using System.Text.RegularExpressions;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace ErogeHelper.ViewModel.Dialogs
{
    public class HCodeViewModel : ReactiveValidationObject
    {
        public Interaction<Unit, string> Show { get; set; } = new();

        public HCodeViewModel()
        {
            var codeValidation = this.ValidationRule(
                vm => vm.HookCode,
                code => Validate(code),
                "Invalid hcode format");

            this.WhenAnyValue(x => x.HookCode,
                code => !string.IsNullOrEmpty(code) && codeValidation.IsValid)
                .ToPropertyEx(this, x => x.CanInsertCode);
        }

        [Reactive]
        public string? HookCode { get; set; } = string.Empty;

        [ObservableAsProperty]
        public bool CanInsertCode { get; }

        public ReactiveCommand<Unit, Unit> SearchCode { get; } = ReactiveCommand.Create(() => { });

        // TODO: Imporve
        private bool Validate(string? code)
        {
            // HCode 0或1个/ H 1个以上任意字符 @ 1个以上十六进制 (: 1个以上任意字符)
            // RCode 0或1个/ RS@ 1个以上十六进制
            if (string.IsNullOrWhiteSpace(code))
            {
                // if hcode is null or space, make TextBox normal style
                return true;
            }

            if (code[^1] == ':')
            {
                return false;
            }

            return Regex.IsMatch(code, ConstantValue.CodeRegExp);
        }
    }
}
