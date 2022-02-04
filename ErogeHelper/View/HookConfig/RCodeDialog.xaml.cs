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

        var disposable = new CompositeDisposable();

        // If the PrimaryButton is disabled, block the "Enter" key
        ContentDialog.Events().Closing
            .Select(x => x.args)
            .Where(args => args.Result == ContentDialogResult.Primary && !ContentDialog.IsPrimaryButtonEnabled)
            .Subscribe(args => args.Cancel = true).DisposeWith(disposable);

        this.WhenActivated(d =>
        {
            disposable.DisposeWith(d);

            this.BindInteraction(ViewModel,
                vm => vm.Show,
                async context =>
                {
                    var dialogResult = await Observable.FromAsync(ContentDialog.ShowAsync);
                    var result = dialogResult switch
                    {
                        ContentDialogResult.Primary => JapaneseText.Text,
                        ContentDialogResult.None => string.Empty,
                        _ => throw new NotImplementedException(),
                    };
                    context.SetOutput(result);
                }).DisposeWith(d);
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
