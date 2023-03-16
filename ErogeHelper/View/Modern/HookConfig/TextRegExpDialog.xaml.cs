using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ErogeHelper.Function;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace ErogeHelper.View.Modern.HookConfig;

public partial class TextRegExpDialog
{
    private new ViewModel.HookConfig.TextRegExpViewModel ViewModel => base.ViewModel!;

    public TextRegExpDialog()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext).DisposeWith(d);

            // If the PrimaryButton is disabled, block the "Enter" key
            ContentDialog.Events().Closing
                .Select(x => x.args)
                .Where(args => args.Result == ContentDialogResult.Primary && !ContentDialog.IsPrimaryButtonEnabled)
                .Subscribe(args => args.Cancel = true).DisposeWith(d);

            ViewModel.DisposeWith(d);

            // Pass data by interation
            this.BindInteraction(ViewModel,
                vm => vm.Show,
                async context =>
                {
                    ViewModel.SelectedHandles = context.Input.Handles;
                    ViewModel.CurrentText = context.Input.Text;

                    ViewModel.CurrentWrapperText
                        = Utils.TextEvaluateWrapperWithRegExp(ViewModel.CurrentText, ViewModel.RegExp);
                    var dialogResult = await Observable.FromAsync(ContentDialog.ShowAsync);

                    context.SetOutput((
                        ContentDialogResult.Primary == dialogResult,
                        ViewModel.RegExp,
                        ViewModel.CurrentText));
                    ViewModel.SelectedHandles = Enumerable.Empty<long>();
                    ViewModel.CurrentText = string.Empty;
                    ViewModel.CurrentWrapperText = string.Empty;
                }).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.RegExp1,
                v => v.RegExp1).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RegExp2,
                v => v.RegExp2).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RegExp3,
                v => v.RegExp3).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RegExp4,
                v => v.RegExp4).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RegExp5,
                v => v.RegExp5).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RegExpClear,
                v => v.RegExpClear).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.CurrentWrapperText,
                v => v.SelectedText.Content,
                TextEvaluate).DisposeWith(d);

            this.BindValidation(ViewModel,
                vm => vm.RegExp,
                v => v.RegExpValidationTip.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.CanSubmit,
                v => v.ContentDialog.IsPrimaryButtonEnabled).DisposeWith(d);
        });
    }

    /// <summary>
    /// Decorations for selected text, need ContentControl to hold it.
    /// </summary>
    /// <param name="input">Input text should wrapper with |~S~| and |~E~|</param>
    private static object TextEvaluate(string input)
    {
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };

        var escapedXml = SecurityElement.Escape(input);

        while (escapedXml?.IndexOf("|~S~|") != -1)
        {
            //up to |~S~| is normal
            textBlock.Inlines.Add(new Run(escapedXml?[..escapedXml.IndexOf("|~S~|", StringComparison.Ordinal)]));

            //between |~S~| and |~E~| is highlighted
            textBlock.Inlines.Add(new Run(escapedXml?[
                (escapedXml.IndexOf("|~S~|", StringComparison.Ordinal) + 5)..escapedXml.IndexOf("|~E~|", StringComparison.Ordinal)])
            {
                TextDecorations = TextDecorations.Strikethrough,
                Background = Brushes.Red
            });

            //the rest of the string (after the |~E~|)
            escapedXml = escapedXml?[(escapedXml.IndexOf("|~E~|", StringComparison.Ordinal) + 5)..];
        }

        if (escapedXml.Length > 0)
            textBlock.Inlines.Add(new Run(escapedXml));
        return textBlock;
    }
}
