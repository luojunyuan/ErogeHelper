using ErogeHelper.Model;
using ErogeHelper.Model.Translator;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// NiuTransDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NiuTransDialog : ContentDialog
    {
        public NiuTransDialog()
        {
            InitializeComponent();

            PrimaryButtonClick += (_, _) =>
            {
                // Condition here cause "Enter" key will cross this
                if (IsPrimaryButtonEnabled)
                {
                    DataRepository.NiuTransApiKey = ApiKey.Text;
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

            ApiKey.Text = DataRepository.NiuTransApiKey;
        }
    }
}
