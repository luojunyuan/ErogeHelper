namespace ErogeHelper.View.Page
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();

            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Page.AboutViewModel>();
        }
    }
}
