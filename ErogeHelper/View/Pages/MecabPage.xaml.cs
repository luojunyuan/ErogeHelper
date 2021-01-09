using System.Windows.Controls;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// MecabPage.xaml 的交互逻辑
    /// </summary>
    public partial class MecabPage : Page
    {
        public MecabPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Pages.MecabViewModel>();
        }
    }
}
