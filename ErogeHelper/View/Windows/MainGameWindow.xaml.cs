using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Windows;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Vanara.PInvoke;

namespace ErogeHelper.View.Windows
{
    public partial class MainGameWindow : IEnableLogger
    {
        private double _dpi;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IMainWindowDataService _mainWindowDataService;

        public MainGameWindow(
            MainGameViewModel? gameViewModel = null,
            IMainWindowDataService? mainWindowDataService = null,
            IGameWindowHooker? gameWindowHooker = null)
        {
            InitializeComponent();

            ViewModel = gameViewModel ?? DependencyInject.GetService<MainGameViewModel>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            _mainWindowDataService.DpiSubject.OnNext(_dpi);
            this.Log().Debug($"Current screen dpi {_dpi * 100}%");

            // QUESTION: This can be used in WhenActivated() with dispose, should I?
            _gameWindowHooker.GamePosUpdated
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

            this.Events().Loaded
                .Subscribe(_ =>
                {
                    Utils.HideWindowInAltTab(this);
                    _gameWindowHooker.InvokeUpdatePosition();
                });

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                    vm => vm.AssistiveTouchViewModel,
                    v => v.AssistiveTouchHost.ViewModel)
                    .DisposeWith(d);
            });
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = newDpi.DpiScaleX;
            this.Log().Debug($"Current screen dpi {_dpi * 100}%");
            _mainWindowDataService.DpiSubject.OnNext(_dpi);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _mainWindowDataService.SetHandle(new HWND(new WindowInteropHelper(this).Handle));
        }
    }
}
