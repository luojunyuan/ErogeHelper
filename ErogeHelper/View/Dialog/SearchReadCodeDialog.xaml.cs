using System.Windows;
using System.Windows.Controls;
using ErogeHelper.Common;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    public partial class SearchReadCodeDialog : ContentDialog
    {
        public SearchReadCodeDialog()
        {
            InitializeComponent();
            
            PrimaryButtonClick += (_, _) =>
            {
                // Condition here cause "Enter" key will cross this
                if (IsPrimaryButtonEnabled)
                {
                    Textractor.SearchRCode(JapaneseText.Text);
                }
            };

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
            if (JapaneseText.Text.Length > 5)
            {
                IsPrimaryButtonEnabled = true;
                TextValidationTip.Visibility = Visibility.Collapsed;
            }
            else if (JapaneseText.Text.Length == 0)
            {
                IsPrimaryButtonEnabled = false;
                TextValidationTip.Visibility = Visibility.Collapsed;
            }
            else
            {
                IsPrimaryButtonEnabled = false;
                TextValidationTip.Visibility = Visibility.Visible;
            }
        }
    }
}