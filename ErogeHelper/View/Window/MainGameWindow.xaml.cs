using System.Reactive.Disposables;
using ErogeHelper.Common;
using ErogeHelper.Common.Extension;
using ErogeHelper.ViewModel.Window;
using ReactiveUI;
using Splat;
using Microsoft.Windows.Sdk;
using System.Windows.Media;
using System.Windows.Interop;
using ErogeHelper.Model.DataService.Interface;
using ErogeHelper.Model.Service.Interface;
using System.Windows;
using System.Reactive.Linq;
using ErogeHelper.Common.Entity;
using System.Windows.Threading;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace ErogeHelper.View.Window
{
    public partial class MainGameWindow : IEnableLogger
    {
        public MainGameWindow(
            MainGameViewModel? gameViewModel = null, 
            IGameDataService? gameDataService = null,
            IGameWindowHooker? gameWindowHooker = null)
        {
            InitializeComponent();

            ViewModel = gameViewModel ?? DependencyInject.GetService<MainGameViewModel>();
            var gameInfo = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;

            Observable
                .FromEventPattern<GameWindowPositionEventArgs>(
                    h => _gameWindowHooker.GamePosChanged += h,
                    h => _gameWindowHooker.GamePosChanged -= h)
                .Select(e => e.EventArgs)
                .Subscribe(async pos => 
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
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
                });

            _gameWindowHooker.InvokeUpdatePosition();

            Loaded += (_, _) => Utils.HideWindowInAltTab(this);

            // https://github.com/reactiveui/ReactiveUI/issues/2395
            this.Log().RxUIWarningTipOnce("Fine exceptions FileNotFoundException " +
                    "reactiveUI is scanning for Drawing, XamForms, Winforms, etc");
            this.WhenActivated(disposableRegistration => { });
        }

        private double _dpi;
        private IGameWindowHooker _gameWindowHooker;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            this.Log().Debug($"Current screen dpi {_dpi * 100}%");
        }

        #region Fullscreen Watcher

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            _handler = new HWND(interopHelper.Handle);

            // Set fullscreen application listener
            RegisterAppBar(false);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            // Always make window front
            _bringToTopTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(50),
            };
            _bringToTopTimer.Tick += (_, _) => PInvoke.BringWindowToTop(_handler);
        }

        private DispatcherTimer? _bringToTopTimer;
        private HWND _desktopHandle;
        private HWND _shellHandle;
        private readonly uint _ehAppbarMsg = PInvoke.RegisterWindowMessage("APPBARMSG_EROGE_HELPER");
        private HWND _handler;

        /// <summary>
        /// Listening to ABNotify.ABN_FULLSCREENAPP message for checking game window handle
        /// </summary>
        /// <param name="registered">false to enable</param>
        private void RegisterAppBar(bool registered)
        {
            var appbarData = new APPBARDATA();
            appbarData.cbSize = (uint)Marshal.SizeOf(appbarData);
            appbarData.hWnd = _handler;

            _desktopHandle = PInvoke.GetDesktopWindow();
            _shellHandle = PInvoke.GetShellWindow();
            if (!registered)
            {
                appbarData.uCallbackMessage = _ehAppbarMsg;
                _ = PInvoke.SHAppBarMessage(ABMsg_ABM_NEW, ref appbarData);
            } 
            else
            {
                _ = PInvoke.SHAppBarMessage(ABMsg_ABM_REMOVE, ref appbarData);
            }
        }

        private const uint ABMsg_ABM_NEW = 0;
        private const uint ABMsg_ABM_REMOVE = 1;
        private const uint ABNotify_ABN_FULLSCREENAPP = 2;

        /// <summary>
        /// Only use for checking game fullscreen status
        /// </summary>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != _ehAppbarMsg)
                return IntPtr.Zero;

            switch (wParam.ToInt32())
            {
                case (int)ABNotify_ABN_FULLSCREENAPP:
                    var foregroundHWnd = PInvoke.GetForegroundWindow();
                    if (foregroundHWnd.Equals(_desktopHandle) || foregroundHWnd.Equals(_shellHandle))
                    {
                        this.Log().Debug("Foregroung is shell or desktop");
                        break;
                    }

                    if ((int)lParam == 1)
                    {
                        this.Log().Debug("The game is being maxsize");
                        _bringToTopTimer?.Start();
                    }
                    else
                    {
                        this.Log().Debug("The game is being normalize or minimize");
                        _bringToTopTimer?.Stop();
                    }
                    _gameWindowHooker.ResetWindowHandler();
                    break;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e) => RegisterAppBar(true);

        #endregion
    }
}
