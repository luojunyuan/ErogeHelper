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
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Translator;
using ErogeHelper.Model.Repository;

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

            _ehConfigRepository = IoC.Get<EhConfigRepository>();

            Appid.Text = _ehConfigRepository.BaiduApiAppid;
            SecretKey.Text = _ehConfigRepository.BaiduApiSecretKey;
            IsPrimaryButtonEnabled = false;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            TestApiButton.IsEnabled = false;
            Appid.IsEnabled = false;
            SecretKey.IsEnabled = false;

            var translator = new BaiduApiTranslator(_ehConfigRepository)
            {
                AppId = Appid.Text, 
                SecretKey = SecretKey.Text
            };
            string result = await translator.TranslateAsync("頼りになる", TransLanguage.日本語, TransLanguage.English);
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
                _ehConfigRepository.BaiduApiAppid = Appid.Text;
                _ehConfigRepository.BaiduApiSecretKey = SecretKey.Text;
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
