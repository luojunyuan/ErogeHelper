using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using ErogeHelper.ViewModel;
using ModernWpf.Controls;
using System;
using System.Runtime.InteropServices;
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

            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            DataRepository.GameViewHandle = interopHelper.Handle;

            RegisterAppBar(false);

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

        private IntPtr desktopHandle;
        private IntPtr shellHandle;
        int uCallBackMsg;

        private void RegisterAppBar(bool registered)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = DataRepository.GameViewHandle;

            desktopHandle = NativeMethods.GetDesktopWindow();
            shellHandle = NativeMethods.GetShellWindow();
            if (!registered)
            {
                //register
                uCallBackMsg = NativeMethods.RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
                abd.uCallbackMessage = uCallBackMsg;
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
            }
            else
            {
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
            }
        }

        private AppBarButton? fullScreenButton;

        private void FullScreenButton_Loaded(object sender, RoutedEventArgs e)
        {
            var appBarbutton = sender as AppBarButton;
            fullScreenButton = appBarbutton;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == uCallBackMsg)
            {
                switch (wParam.ToInt32())
                {
                    case (int)NativeMethods.ABNotify.ABN_FULLSCREENAPP:
                        IntPtr hWnd = NativeMethods.GetForegroundWindow();
                        //判断当前全屏的应用是否是桌面
                        if (hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle))
                        {
                            break;
                        }
                        //判断是否全屏
                        if ((int)lParam == 1)
                        {
                            Log.Info("The window is being maxsize");
                            if (fullScreenButton is not null)
                            {
                                fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.BackToWindow };
                                fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchWindow;
                            }
                        }
                        else
                        {
                            Log.Info("The window is being normalize or minimize");
                            if (fullScreenButton is not null)
                            {
                                fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.FullScreen };
                                fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchFullScreen;
                            }
                        }
                        GameHooker.CheckWindowHandler();
                        break;
                    default:
                        break;
                }
            }
            
            return IntPtr.Zero;
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
