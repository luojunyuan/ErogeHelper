using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ModernWpf.Controls;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ErogeHelper.Common.Function;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// Interaction logic for InsideView.xaml
    /// </summary>
    public partial class InsideView : IHandle<ViewActionMessage>, IHandle<LoseFocusMessage>
    {
        public InsideView()
        {
            InitializeComponent();

            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
            Visibility = Visibility.Collapsed;
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            _gameWindowHooker = IoC.Get<IGameWindowHooker>();
            _gameWindowHooker.GamePosArea += PositionChanged;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        private readonly IGameWindowHooker _gameWindowHooker;
        private double _dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Height = pos.Height / _dpi;
                Width = pos.Width / _dpi;
                Left = pos.Left / _dpi;
                Top = pos.Top / _dpi;
                ClientArea.Margin = new Thickness(
                    pos.ClientArea.Left / _dpi,
                    pos.ClientArea.Top / _dpi,
                    pos.ClientArea.Right / _dpi,
                    pos.ClientArea.Bottom / _dpi);
            });
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            Log.Debug($"Current screen dpi {_dpi * 100}%");
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Show:
                        _gameWindowHooker.InvokeLastWindowPosition();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            var globalValues = IoC.Get<GameRuntimeInfoRepository>();
            _handler = interopHelper.Handle;

            // Set fullscreen application listener
            RegisterAppBar(false);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            // Always make window front
            // UNDONE: Test if can use in both two windows
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(50),
            };
            timer.Tick += (_, _) =>
            {
                if (_handler == IntPtr.Zero)
                {
                    timer.Stop();
                }
                // UNDONE
                // 不加这个条件？ 另外这些其实是在有了工具窗口判断全屏前写的，后来有了判断全屏就没改过这些
                // 全屏判断相当稳定、考虑一下，以及计时器的停止啥的
                if (globalValues.MainProcess.MainWindowHandle == NativeMethods.GetForegroundWindow())
                {
                    NativeMethods.BringWindowToTop(_handler);
                    //Log.Debug("yes");
                }
            };
        }

        private IntPtr _desktopHandle;
        private IntPtr _shellHandle;
        private readonly int _appbarMsg = NativeMethods.RegisterWindowMessage("APPBARMSG_EROGE_HELPER");
        private IntPtr _handler;

        /// <summary>
        /// Listening to ABNotify.ABN_FULLSCREENAPP message for checking game window handle
        /// </summary>
        /// <param name="registered">false to enable</param>
        private void RegisterAppBar(bool registered)
        {
            var abd = new NativeMethods.AppbarData();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = _handler;

            _desktopHandle = NativeMethods.GetDesktopWindow();
            _shellHandle = NativeMethods.GetShellWindow();
            if (!registered)
            {
                abd.uCallbackMessage = _appbarMsg;
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
            }
            else
            {
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
            }
        }

        private AppBarButton? _fullScreenButton;

        private void FullScreenButton_Loaded(object sender, RoutedEventArgs e)
        {
            var appbarButton = sender as AppBarButton;
            _fullScreenButton = appbarButton;
        }

        /// <summary>
        /// Only use for checking game fullscreen status
        /// </summary>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != _appbarMsg)
                return IntPtr.Zero;

            switch (wParam.ToInt32())
            {
                case (int)NativeMethods.ABNotify.ABN_FULLSCREENAPP:
                    var foregroundHWnd = NativeMethods.GetForegroundWindow();
                    //判断当前全屏的应用是否是桌面
                    if (foregroundHWnd.Equals(_desktopHandle) || foregroundHWnd.Equals(_shellHandle))
                    {
                        break;
                    }
                    //判断是否全屏
                    if ((int)lParam == 1)
                    {
                        Log.Debug("The front window is being maxsize");

                        if (_fullScreenButton is not null)
                        {
                            _fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.BackToWindow };
                            _fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchWindow;
                        }
                    }
                    else
                    {
                        Log.Debug("The front window is being normalize or minimize");
                        if (_fullScreenButton is not null)
                        {
                            _fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.FullScreen };
                            _fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchFullScreen;
                        }
                    }
                    _gameWindowHooker.ResetWindowHandler();
                    break;
            }

            return IntPtr.Zero;
        }

        public Task HandleAsync(LoseFocusMessage lostFocus, CancellationToken cancellationToken)
        {
            if (lostFocus.Status)
            {
                var exStyle = NativeMethods.GetWindowLong(_handler, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_handler, 
                    NativeMethods.GWL_EXSTYLE, 
                    exStyle | NativeMethods.WS_EX_NOACTIVATE);
            }
            else
            {
                var exStyle = NativeMethods.GetWindowLong(_handler, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_handler,
                    NativeMethods.GWL_EXSTYLE,
                    exStyle & ~NativeMethods.WS_EX_NOACTIVATE);
            }
            return Task.CompletedTask;
        }
    }
}
