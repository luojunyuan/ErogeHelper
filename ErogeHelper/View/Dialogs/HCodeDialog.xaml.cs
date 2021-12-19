using System.Reactive.Disposables;
using ModernWpf.Controls;
using ReactiveUI;

namespace ErogeHelper.View.Dialogs
{
    public partial class HCodeDialog
    {
        public HCodeDialog()
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
}