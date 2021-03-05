using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.View.Dialog;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// HookSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class HookPage : System.Windows.Controls.Page
    {
        public HookPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.HookViewModel>();
        }

        private async void CodeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CodeButton.IsEnabled = false;
            RCodeButton.IsEnabled = false;

            await CodeDialog.ShowAsync();

            CodeButton.IsEnabled = true;
            RCodeButton.IsEnabled = true;
        }

        private async void RCodeButton_OnClick(object sender, RoutedEventArgs e) => await new SearchReadCodeDialog().ShowAsync();
    }
}
