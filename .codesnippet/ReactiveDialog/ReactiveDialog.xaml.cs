using ModernWpf.Controls;
using System.Reactive.Disposables;
using ReactiveUI;

namespace $rootnamespace$;

public partial class $safeitemname$
{
    public $safeitemname$()
    {
        InitializeComponent();

        ContentDialog.PrimaryButtonClick += (_, _) =>
        {
            // Condition here cause "Enter" key will cross this
            if (ContentDialog.IsPrimaryButtonEnabled)
            {
            }
        };

        ContentDialog.Closing += (_, args) =>
        {
            // If the PrimaryButton is disabled, block the "Enter" key
            if (args.Result == ContentDialogResult.Primary && !ContentDialog.IsPrimaryButtonEnabled)
            {
                args.Cancel = true;
            }
        };

        this.WhenActivated(d =>
        {

        });
    }
}
