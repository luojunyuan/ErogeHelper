using ErogeHelper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// TextView.xaml 的交互逻辑
    /// </summary>
    public partial class TextControl : UserControl
    {
        public TextControl()
        {
            InitializeComponent();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            border!.Effect = new DropShadowEffect
            {
                Color = new Color { A = 255, R = 255, G = 255, B = 0 },
                Direction = 320,
                ShadowDepth = 0,
                Opacity = 1
            };
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            border!.ClearValue(EffectProperty);
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            border!.Opacity = 0.5;
        }

        private void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            Popup? popup = Resources["CardPopup"] as Popup;
            popup!.PlacementTarget = border;
            popup!.IsOpen = true;
        }

        private void Border_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            border!.Opacity = 1;
        }
    }
}
