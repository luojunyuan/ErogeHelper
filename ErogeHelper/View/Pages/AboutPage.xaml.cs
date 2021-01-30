using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.AboutViewModel>();
        }
    }
}
