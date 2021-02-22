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
    /// BaiduApiDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BaiduApiDialog : ContentDialog
    {
        public BaiduApiDialog()
        {
            InitializeComponent();

            Appid.Text = DataRepository.BaiduApiAppid;
            SecretKey.Text = DataRepository.BaiduApiSecretKey;
            IsPrimaryButtonEnabled = false;
        }

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            TestApiButton.IsEnabled = false;
            Appid.IsEnabled = false;
            SecretKey.IsEnabled = false;

            var translator = new BaiduApiTranslator();
            translator.appId = Appid.Text;
            translator.secretKey = SecretKey.Text;
            string result = await translator.TranslateAsync("頼りになる", Languages.日本語, Languages.English);
            if (result.Equals("Rely on"))
            {
                IsPrimaryButtonEnabled = true;
                TestStatus.Text = "√";
                TestStatus.Foreground = Brushes.Green;
            }
            else
            {
                IsPrimaryButtonEnabled = false;
                TestStatus.Text = "X";
                TestStatus.Foreground = Brushes.Red;
                Appid.IsEnabled = true;
                SecretKey.IsEnabled = true;
            }

            TestApiButton.IsEnabled = true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsPrimaryButtonEnabled)
            {
                DataRepository.BaiduApiAppid = Appid.Text;
                DataRepository.BaiduApiSecretKey = SecretKey.Text;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
            {
                args.Cancel = true;
            }
        }
    }
}
