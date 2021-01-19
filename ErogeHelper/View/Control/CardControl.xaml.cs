using Caliburn.Micro;
using ErogeHelper.ViewModel.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            DataContext = IoC.Get<CardViewModel>();
        }

        #region Disable White Point by Touch
        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);
            Cursor = Cursors.None;
        }
        protected override void OnPreviewTouchMove(TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);
            Cursor = Cursors.None;
        }
        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);
            Cursor = Cursors.Arrow;
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (e.StylusDevice == null)
                Cursor = Cursors.Arrow;
        }
        #endregion
    }
}
