using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
        private double _dpi;

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

        public MainGameViewModel(
            AssistiveTouchViewModel? assistiveTouchViewModel = null,
            IEhConfigRepository? ehConfigRepository = null,
            IMainWindowDataService? mainWindowDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IGameInfoRepository? ehDbRepository = null)
        {
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyResolver.GetService<AssistiveTouchViewModel>();
            gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
            mainWindowDataService ??= DependencyResolver.GetService<IMainWindowDataService>();
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();
            ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

            _dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow).DpiScaleX;

            gameWindowHooker.GamePosUpdated
                .Subscribe(pos =>
                {
                    Height = pos.Height / _dpi;
                    Width = pos.Width / _dpi;
                    Left = pos.Left / _dpi;
                    Top = pos.Top / _dpi;
                    ClientAreaMargin = new Thickness(
                        pos.ClientArea.Left / _dpi,
                        pos.ClientArea.Top / _dpi,
                        pos.ClientArea.Right / _dpi,
                        pos.ClientArea.Bottom / _dpi);
                });
            gameWindowHooker.InvokeUpdatePosition();

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;

            LoadedCommand = ReactiveCommand.Create(() =>
            {
                mainWindowDataService.SetHandle(MainWindowHandle);
                Utils.HideWindowInAltTab(MainWindowHandle);
                gameWindowHooker.InvokeUpdatePosition();

                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(MainWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });

            DpiChangedCommand = ReactiveCommand.Create<double>(dpi =>
            {
                this.Log().Debug($"Current screen dpi {dpi * 100}%");
                _dpi = dpi;

                Observable.Create<Unit>(observer =>
                {
                    observer.OnNext(Unit.Default);
                    return Disposable.Empty;
                })
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    mainWindowDataService.DpiSubject.OnNext(dpi);
                    gameWindowHooker.InvokeUpdatePosition();
                });
            });
        }

        public HWND MainWindowHandle { get; set; }

        [Reactive]
        public double Height { get; set; }

        [Reactive]
        public double Width { get; set; }

        [Reactive]
        public double Left { get; private set; }

        [Reactive]
        public double Top { get; private set; }

        [Reactive]
        public Thickness ClientAreaMargin { get; private set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }

        public ReactiveCommand<Unit, Unit> LoadedCommand { get; init; }

        public ReactiveCommand<double, Unit> DpiChangedCommand { get; init; }
    }
}
