using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// Interaction logic for InsideView.xaml
    /// </summary>
    public partial class InsideView
    {
        public InsideView()
        {
            InitializeComponent();

            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            IoC.Get<IGameWindowHooker>().GamePosArea += PositionChanged;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        private double dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Height = pos.Height / dpi;
                Width = pos.Width / dpi;
                Left = pos.Left / dpi;
                Top = pos.Top / dpi;
                ClientArea.Margin = new Thickness(
                    pos.ClientArea.Left / dpi,
                    pos.ClientArea.Top / dpi,
                    pos.ClientArea.Right / dpi,
                    pos.ClientArea.Bottom / dpi);
            });
        }
    }
}
