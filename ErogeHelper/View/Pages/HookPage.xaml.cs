using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// HookSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class HookPage : Page
    {
        public HookPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.HookViewModel>();
        }
    }
}
