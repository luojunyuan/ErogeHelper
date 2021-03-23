using ErogeHelper.Common;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// OutsideView.xaml 的交互逻辑
    /// </summary>
    public partial class OutsideView
    {
        public OutsideView()
        {
            InitializeComponent();

            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }
    }
}
