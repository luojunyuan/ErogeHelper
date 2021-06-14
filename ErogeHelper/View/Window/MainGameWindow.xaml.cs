using ErogeHelper.Common;
using ErogeHelper.ViewModel.Window;
using ReactiveUI;
using Splat;
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
using ErogeHelper.Common.Contract;
using ErogeHelper.Common.Enum;
using Vanara.PInvoke;

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
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
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

            //_gameWindowHooker.InvokeUpdatePosition();

            Loaded += (_, _) => Utils.HideWindowInAltTab(this);

            this.WhenActivated(disposableRegistration => { });
        }

        private double _dpi;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IGameDataService _gameDataService;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = newDpi.DpiScaleX;
            this.Log().Debug($"Current screen dpi {_dpi * 100}%");
        }

        #region Game Window Status Watcher

        private DispatcherTimer? _bringToTopTimer;
        private HWND _handler;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            _handler = new HWND(interopHelper.Handle);

            var source = PresentationSource.FromVisual(this) as HwndSource;

            // Window top most timer
            _bringToTopTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime),
            };
            _bringToTopTimer.Tick += (_, _) => User32.BringWindowToTop(_handler);

            //if (IsGameForegroundFullScreen(_gameWindowHooker.GameRealHwnd))
            //{
            //    _bringToTopTimer.Start();
            //    ClientArea.BorderBrush = Brushes.Red;
            //}
            
            //if (User32.IsIconic(_gameWindowHooker.GameRealHwnd))
            //{
            //    _gameDataService.IsMinimized = true;
            //}
            //else
            //{
            //    _gameDataService.IsMinimized = false;
            //}

            Observable.Interval(TimeSpan.FromMilliseconds(ConstantValues.GameWindowStatusRefreshTime))
                .Subscribe(_ =>
                {
                    // valid

                    // visible

                    // rect

                        // state
                    //WINDOWPLACEMENT ws = new();
                    //PInvoke.GetWindowPlacement(_handler, ref ws);
                    //switch(ws.showCmd)
                    //{ 
                    //    case SHOW_WINDOW_CMD.SW_SHOWNORMAL:
                    //        this.Log().Debug(ws.showCmd);
                    //        break;
                    //    case SHOW_WINDOW_CMD.SW_SHOWMINIMIZED:
                    //        this.Log().Debug(ws.showCmd);
                    //        break;
                    //    case SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED:
                    //        this.Log().Debug(ws.showCmd);
                    //        break;
                    // }
                    //Vanara.PInvoke.Shell32.SHAppBarMessage(Vanara.PInvoke.Shell32.ABM.ABM_ACTIVATE,  );
                    //if (PInvoke.IsZoomed(_gameWindowHooker.GameRealHwnd))
                    //{
                    //    currentStatus = WindowStatus.Fullscreen;
                    //}
                    //else if (PInvoke.IsIconic(_gameWindowHooker.GameRealHwnd))
                    //{
                    //    currentStatus = WindowStatus.Minimized;
                    //}
                    //else
                    //{
                    //    currentStatus = WindowStatus.Normal;
                    //}

                    //if (currentStatus != _gameDataService.WindowsPlacement)
                    //{
                    //    switch(currentStatus)
                    //    {
                    //        case WindowStatus.Normal:
                    //            this.Log().Debug("normal");
                    //            break;
                    //        case WindowStatus.Fullscreen:
                    //            this.Log().Debug("fullscreen");
                    //            break;
                    //        case WindowStatus.Minimized:
                    //            this.Log().Debug("minimized");
                    //            break;
                    //    }

                    //    _gameDataService.WindowsPlacement = currentStatus;
                    //}
                });
        }

        private static bool IsGameForegroundFullScreen(IntPtr gameHwnd)
        { 
            foreach(var screen in WpfScreenHelper.Screen.AllScreens)
            {
                User32.GetWindowRect(gameHwnd, out var rect);
                var systemRect = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                if (systemRect.Contains(screen.Bounds))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
