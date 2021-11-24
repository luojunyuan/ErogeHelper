using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.Share.Enums;
using ErogeHelper.Share.Structs;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject
    {
        public TouchToolBoxViewModel TouchToolBoxViewModel { get; }

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

        // public TextViewModel InsideTextViewModel { get; }

        public MainGameViewModel(
            AssistiveTouchViewModel? assistiveTouchViewModel = null,
            TouchToolBoxViewModel? touchToolBoxViewModel = null,
            IEHConfigRepository? ehConfigRepository = null,
            IWindowDataService? windowDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IGameInfoRepository? ehDbRepository = null,
            IGameDataService? gameDataService = null)
        {
            TouchToolBoxViewModel = touchToolBoxViewModel ?? DependencyResolver.GetService<TouchToolBoxViewModel>();
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyResolver.GetService<AssistiveTouchViewModel>();
            gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
            windowDataService ??= DependencyResolver.GetService<IWindowDataService>();
            ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
            ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
            gameDataService ??= DependencyResolver.GetService<IGameDataService>();

            ehConfigRepository.WhenAnyValue(x => x.UseEdgeTouchMask)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ShowEdgeTouchMask);

            gameWindowHooker.GamePosUpdated
                .Subscribe(pos =>
                {
                    Height = pos.Height / Dpi;
                    Width = pos.Width / Dpi;
                    Left = pos.Left / Dpi;
                    Top = pos.Top / Dpi;
                    ClientAreaMargin = new Thickness(
                        pos.ClientArea.Left / Dpi,
                        pos.ClientArea.Top / Dpi,
                        pos.ClientArea.Right / Dpi,
                        pos.ClientArea.Bottom / Dpi);
                });

            gameWindowHooker.WhenViewOperated
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(operation =>
                {
                    switch (operation)
                    {
                        case ViewOperation.Show:
                            ShowMainWindow.Handle(Unit.Default).ObserveOn(RxApp.MainThreadScheduler).Subscribe();
                            break;
                        case ViewOperation.Hide:
                            HideMainWindow.Handle(Unit.Default).ObserveOn(RxApp.MainThreadScheduler).Subscribe();
                            break;
                        case ViewOperation.TerminateApp:
                            Interactions.TerminateApp.Handle(Unit.Default)
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe();
                            break;
                    }
                });

            #region BringWindowToTop Subject
            
            var stayTopSubj =
                new BehaviorSubject<bool>(Interactions.
                    CheckGameFullscreen.Handle(gameDataService.GameRealWindowHandle).Wait());

            var interval = Observable
                .Interval(TimeSpan.FromMilliseconds(ConstantValue.GameFullscreenStatusRefreshTime))
                .TakeUntil(stayTopSubj.Where(on => !on));

            stayTopSubj
                .DistinctUntilChanged()
                .Where(on => on && !windowDataService.MainWindowHandle.IsNull)
                .SelectMany(interval)
                .Subscribe(_ => User32.BringWindowToTop(windowDataService.MainWindowHandle));

            #endregion

            gameDataService.GameFullscreenChanged
                .Do(isFullscreen => this.Log().Debug("Game fullscreen: " + isFullscreen))
                .Subscribe(isFullscreen => stayTopSubj.OnNext(isFullscreen));
            
            Loaded = ReactiveCommand.Create(() =>
            {
                windowDataService.InitMainWindowHandle(MainWindowHandle);
                HwndTools.HideWindowInAltTab(MainWindowHandle);
                // The first time position get invoked
                gameWindowHooker.InvokeUpdatePosition();

                if (ehDbRepository.GameInfo.IsLoseFocus)
                {
                    HwndTools.WindowLostFocus(MainWindowHandle, ehDbRepository.GameInfo.IsLoseFocus);
                }
            });

            DpiChanged = ReactiveCommand.Create<double>(newDpi =>
            {
                this.Log().Debug($"Current screen dpi {newDpi * 100}%");
                Dpi = newDpi;

                // HACK: EH may receive the dpi changed event faster than the game initialization
                // when starting for the first time 
                Observable
                    .Start(() => Unit.Default)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => gameWindowHooker.InvokeUpdatePosition());
            });
        }

        public double Dpi { private get; set; }

        public HWND MainWindowHandle { private get; set; }

        [Reactive] 
        public double Height { get; set; }

        [Reactive] 
        public double Width { get; set; }

        [Reactive] 
        public double Left { get; set; }

        [Reactive] 
        public double Top { get; set; }

        [Reactive] 
        public Thickness ClientAreaMargin { get; set; }

        [ObservableAsProperty]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool ShowEdgeTouchMask { get; }

        public ReactiveCommand<Unit, Unit> Loaded { get; }

        public ReactiveCommand<double, Unit> DpiChanged { get; }

        public Interaction<Unit, Unit> ShowMainWindow { get; } = new();

        public Interaction<Unit, Unit> HideMainWindow { get; } = new();
    }
}
