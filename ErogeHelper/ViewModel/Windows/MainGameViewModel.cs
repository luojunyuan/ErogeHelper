using ErogeHelper.Common.Enums;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
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

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;

            var dpi = WpfScreenHelper.Screen
                .FromHandle(gameDataService.GameRealWindowHandle.DangerousGetHandle())
                .ScaleFactor;
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

            gameWindowHooker.WindowOperationSubj
                .Subscribe(operation =>
                {
                    switch (operation)
                    {
                        case WindowOperation.Show:
                            _showSubj.OnNext(Unit.Default);
                            break;
                        case WindowOperation.Hide:
                            _hideSubj.OnNext(Unit.Default);
                            break;
                        case WindowOperation.TerminateApp:
                            _terminateAppSubj.OnNext(Unit.Default);
                            break;
                        default:
                            break;
                    }
                });

            Loaded = ReactiveCommand.Create(() =>
            {
                mainWindowDataService.SetHandle(MainWindowHandle);
                Utils.HideWindowInAltTab(MainWindowHandle);
                gameWindowHooker.InvokeUpdatePosition();

                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(MainWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });

            DpiChanged = ReactiveCommand.Create<double>(newDpi =>
            {
                this.Log().Debug($"Current screen dpi {newDpi * 100}%");
                dpi = newDpi;

                // This is hack
                Observable
                    .Start(() => Unit.Default)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                    {
                        AssistiveTouchViewModel.DpiSubj.OnNext(newDpi);
                        gameWindowHooker.InvokeUpdatePosition();
                    });
            });
        }

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

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

        public ReactiveCommand<Unit, Unit> Loaded { get; init; }

        public ReactiveCommand<double, Unit> DpiChanged { get; init; }

        private readonly Subject<Unit> _showSubj = new();
        public IObservable<Unit> ShowSubj => _showSubj;

        private readonly Subject<Unit> _hideSubj = new();
        public IObservable<Unit> HideSubj => _hideSubj;

        private readonly Subject<Unit> _terminateAppSubj = new();
        public IObservable<Unit> TerminateAppSubj => _terminateAppSubj;
    }
}
