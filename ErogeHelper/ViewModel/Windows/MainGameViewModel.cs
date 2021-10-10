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
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

        public MainGameViewModel(
            AssistiveTouchViewModel? assistiveTouchViewModel = null,
            IEhConfigRepository? ehConfigRepository = null,
            IMainWindowDataService? mainWindowDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IGameInfoRepository? ehDbRepository = null,
            IGameDataService? gameDataService = null)
        {
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyResolver.GetService<AssistiveTouchViewModel>();
            gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
            mainWindowDataService ??= DependencyResolver.GetService<IMainWindowDataService>();
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();
            ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
            gameDataService ??= DependencyResolver.GetService<IGameDataService>();

            var currentScreen = WpfScreenHelper.Screen.FromHandle(gameDataService.MainWindowHandle.DangerousGetHandle());
            var dpi = currentScreen.ScaleFactor;

            gameWindowHooker.GamePosUpdated
                .Subscribe(pos =>
                {
                    Height = pos.Height / dpi;
                    Width = pos.Width / dpi;
                    Left = pos.Left / dpi;
                    Top = pos.Top / dpi;
                    ClientAreaMargin = new System.Windows.Thickness(
                        pos.ClientArea.Left / dpi,
                        pos.ClientArea.Top / dpi,
                        pos.ClientArea.Right / dpi,
                        pos.ClientArea.Bottom / dpi);
                });
            gameWindowHooker.InvokeUpdatePosition();

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;

            LoadedCommand = ReactiveCommand.Create(() =>
            {
                mainWindowDataService.SetHandle(MainWindowHandle);
                gameWindowHooker.InvokeUpdatePosition();

                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(MainWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });

            DpiChangedCommand = ReactiveCommand.Create<double>(newDpi =>
            {
                this.Log().Debug($"Current screen dpi {newDpi * 100}%");
                dpi = newDpi;

                Observable.Create<Unit>(observer =>
                {
                    observer.OnNext(Unit.Default);
                    return Disposable.Empty;
                })
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    mainWindowDataService.DpiSubject.OnNext(newDpi);
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
        public System.Windows.Thickness ClientAreaMargin { get; private set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }

        public ReactiveCommand<Unit, Unit> LoadedCommand { get; init; }

        public ReactiveCommand<double, Unit> DpiChangedCommand { get; init; }
    }
}
