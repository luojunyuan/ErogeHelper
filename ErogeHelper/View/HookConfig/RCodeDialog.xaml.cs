using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using ReactiveUI;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;

namespace ErogeHelper.View.HookConfig;

public partial class RCodeDialog
{
    public RCodeDialog()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            // If the PrimaryButton is disabled, block the "Enter" key
            ContentDialog.Events().Closing
                .Select(x => x.args)
                .Where(args => args.Result == ContentDialogResult.Primary && !ContentDialog.IsPrimaryButtonEnabled)
                .Subscribe(args => args.Cancel = true).DisposeWith(d);

            this.BindInteraction(ViewModel,
                vm => vm.Show,
                async context => context.SetOutput(ContentDialogResult.Primary ==
                    await Observable.FromAsync(ContentDialog.ShowAsync) ?
                        JapaneseText.Text : string.Empty)).DisposeWith(d);
        });
    }

    private void JapaneseTextOnTextChanged(object sender, TextChangedEventArgs e)
    {
        switch (JapaneseText.Text.Length)
        {
            case > 5:
                ContentDialog.IsPrimaryButtonEnabled = true;
                TextValidationTip.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                break;
            case 0:
                ContentDialog.IsPrimaryButtonEnabled = false;
                TextValidationTip.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                break;
            default:
                ContentDialog.IsPrimaryButtonEnabled = false;
                TextValidationTip.SetCurrentValue(VisibilityProperty, Visibility.Visible);
                break;
        }
    }
}
