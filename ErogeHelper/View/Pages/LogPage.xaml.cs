using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// LogPage.xaml 的交互逻辑
    /// </summary>
    public partial class LogPage : Page
    {
        public LogPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.LogViewModel>();
        }
    }
}
