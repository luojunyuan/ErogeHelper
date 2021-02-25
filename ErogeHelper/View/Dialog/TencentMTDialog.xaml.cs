using ErogeHelper.Model;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// TencentMTDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TencentMTDialog : ContentDialog
    {
        public TencentMTDialog()
        {
            InitializeComponent();

            PrimaryButtonClick += (_, _) =>
            {
                // Condition here cause "Enter" key will cross this
                if (IsPrimaryButtonEnabled)
                {
                    DataRepository.TencentMTSecretId = SecretId.Text;
                    DataRepository.TencentMTSecretKey = SecretKey.Text;
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

            SecretId.Text = DataRepository.TencentMTSecretId;
            SecretKey.Text = DataRepository.TencentMTSecretKey;
        }
    }
}
