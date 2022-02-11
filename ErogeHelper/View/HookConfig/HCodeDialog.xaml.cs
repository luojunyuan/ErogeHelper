using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace ErogeHelper.View.HookConfig;

public partial class HCodeDialog
{
    public HCodeDialog()
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

            CodeTextBox.Events().PreviewKeyDown
                .Where(e => e.Key == Key.Space)
                .Subscribe(e => e.Handled = true).DisposeWith(d);

            this.BindValidation(ViewModel,
                vm => vm.HookCode,
                v => v.CodeValidationTip.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.CanInsertCode,
                v => v.ContentDialog.IsPrimaryButtonEnabled).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.SearchCode,
                v => v.SearchCodeButton).DisposeWith(d);

            this.BindInteraction(ViewModel,
                vm => vm.Show,
                async context => context.SetOutput(ContentDialogResult.Primary ==
                    await Observable.FromAsync(ContentDialog.ShowAsync) ?
                        CodeTextBox.Text : string.Empty)).DisposeWith(d);
        });
    }
}
