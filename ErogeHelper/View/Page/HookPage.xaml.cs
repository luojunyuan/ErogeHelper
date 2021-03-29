namespace ErogeHelper.View.Page
{
    /// <summary>
    /// HookPage.xaml 的交互逻辑
    /// </summary>
    public partial class HookPage
    {
        public HookPage()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Page.HookViewModel>();
        }
    }
}
