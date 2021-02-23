using ErogeHelper.Model;
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
    /// CaiyunDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CaiyunDialog : ContentDialog
    {
        public CaiyunDialog()
        {
            InitializeComponent();

            Token.Text = DataRepository.CaiyunToken;
            Token_TextChanged(null!, null!);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsPrimaryButtonEnabled)
            {
                DataRepository.CaiyunToken = Token.Text;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
            {
                args.Cancel = true;
            }
        }

        private void Token_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!Token.Text.Equals(string.Empty))
            {
                IsPrimaryButtonEnabled = true;
            }
            else
            {
                IsPrimaryButtonEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Token.Text = DefaultValuesStore.CaiyunDefaultToken;
        }
    }
}
