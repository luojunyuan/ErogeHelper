using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ErogeHelper.View
{
    /// <summary>
    /// GameView.xaml 的交互逻辑
    /// </summary>
    public partial class GameView : Window
    {
        private double dpi;

        public GameView()
        {
            InitializeComponent();

            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
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

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            Log.Info($"Current screen dpi {dpi * 100}%");
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
