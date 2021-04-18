using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// RCodeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RCodeDialog
    {
        public RCodeDialog()
        {
            InitializeComponent();

            Closing += (_, args) =>
            {
                // If the PrimaryButton is disabled, block the "Enter" key
                if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
                {
                    args.Cancel = true;
                }
            };
        }

        private void JapaneseText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            switch (JapaneseText.Text.Length)
            {
                case > 5:
                    IsPrimaryButtonEnabled = true;
                    TextValidationTip.Visibility = Visibility.Collapsed;
                    break;
                case 0:
                    IsPrimaryButtonEnabled = false;
                    TextValidationTip.Visibility = Visibility.Collapsed;
                    break;
                default:
                    IsPrimaryButtonEnabled = false;
                    TextValidationTip.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
