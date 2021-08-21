using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Vanara.PInvoke;

namespace ErogeHelper.View.Windows
{
    public partial class MainGameWindow : IEnableLogger
    {
        private double _dpi;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IGameDataService _gameDataService;

        // TODO: Use EHWindowDataService ? 东西多了再用
        private HWND _handler;

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
                .Subscribe(pos =>
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

            _gameWindowHooker.InvokeUpdatePosition();

            Loaded += (_, _) => Utils.HideWindowInAltTab(this);
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel,
                    vm => vm.Interact,
                    v => v.InteractButton)
                    .DisposeWith(d);
            });
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = newDpi.DpiScaleX;
            this.Log().Debug($"Current screen dpi {_dpi * 100}%");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            _handler = new HWND(interopHelper.Handle);

            var source = PresentationSource.FromVisual(this) as HwndSource;

            // Window top most timer
            var _bringToTopTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime),
            };
            _bringToTopTimer.Tick += (_, _) => User32.BringWindowToTop(_handler);
            // TODO: 是在VM中绑定过来 还是定义在VM中 还是把Timer放到那边 应该用rx此处
            //_bringToTopTimer.Start();
            //_bringToTopTimer.Stop();
        }

        private static bool IsGameForegroundFullScreen(IntPtr gameHwnd)
        {
            foreach (var screen in WpfScreenHelper.Screen.AllScreens)
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
    }
}
