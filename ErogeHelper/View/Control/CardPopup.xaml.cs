namespace ErogeHelper.View.Control
{
    /// <summary>
    /// CardPopup.xaml 的交互逻辑
    /// </summary>
    public partial class CardPopup
    {
        public CardPopup()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Control.CardViewModel>();
        }
    }
}
