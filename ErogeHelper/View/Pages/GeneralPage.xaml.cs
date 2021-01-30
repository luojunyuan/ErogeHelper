using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// GeneralSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class GeneralPage : Page
    {
        public GeneralPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.GeneralViewModel>();
        }
    }
}
