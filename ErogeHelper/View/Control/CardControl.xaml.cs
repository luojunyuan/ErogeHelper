using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// CardControl.xaml 的交互逻辑
    /// </summary>
    public partial class CardControl : UserControl
    {
        public CardControl()
        {
            InitializeComponent();
            DataContext = Caliburn.Micro.IoC.Get<ViewModel.Control.CardViewModel>();
        }
    }
}
