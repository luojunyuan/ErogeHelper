using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ErogeHelper.Views
{
    /// <summary>
    /// GameView.xaml 的交互逻辑
    /// </summary>
    public partial class GameView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameView));

        public GameView()
        {
            InitializeComponent();

            DataRepository.dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            GameHooker.GameViewPosChangedEvent += PositionChanged;
            Loaded += GameView_Loaded;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            HideAltTab();
        }

        private void HideAltTab()
        {
            var windowInterop = new WindowInteropHelper(this);
            var exStyle = NativeMethods.GetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE);
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE, exStyle);
        }

        private void PositionChanged(object sender, GameViewPlacement pos)
        {
            Height = pos.Height;
            Width = pos.Width;
            Left = pos.Left;
            Top = pos.Top;
            ClientArea.Margin = pos.ClientArea;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            DataRepository.dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            log.Info($"Current screen dpi {DataRepository.dpi * 100}%");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            DataRepository.GameViewHandle = interopHelper.Handle;

            // Alaways make window front
            DispatcherTimer timer = new DispatcherTimer();
            var pointer = new WindowInteropHelper(this);
            timer.Tick += (sender, _) =>
            {
                if (pointer.Handle == IntPtr.Zero)
                {
                    timer.Stop();
                }
                if (DataRepository.MainProcess?.MainWindowHandle == NativeMethods.GetForegroundWindow())
                {
                    NativeMethods.BringWindowToTop(pointer.Handle);
                }
            };

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Start();
        }
    }
}
