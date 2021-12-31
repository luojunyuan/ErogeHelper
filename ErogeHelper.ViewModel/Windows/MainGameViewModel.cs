using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows;

public class MainGameViewModel : ReactiveObject, IDisposable
{
    public TouchToolBoxViewModel TouchToolBoxViewModel { get; }

    public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

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
            .ToPropertyEx(this, x => x.ShowEdgeTouchMask)
            .DisposeWith(_disposables);

        gameWindowHooker.GamePosUpdated
            .Subscribe(pos =>
            {
                Height = pos.Height / Dpi;
                Width = pos.Width / Dpi;
                Left = pos.Left / Dpi;
                Top = pos.Top / Dpi;
            }).DisposeWith(_disposables);

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
            }).DisposeWith(_disposables);

        #region BringWindowToTop Subject

        var stayTopSubj = new Subject<bool>();

        var interval = Observable
            .Interval(TimeSpan.FromMilliseconds(ConstantValue.GameFullscreenStatusRefreshTime))
            .TakeUntil(stayTopSubj.Where(on => !on));

        stayTopSubj
            .DistinctUntilChanged()
            .Where(on => on)
            .SelectMany(interval)
            .Where(_ => !windowDataService.MainWindowHandle.IsNull)
            .Subscribe(_ => User32.BringWindowToTop(windowDataService.MainWindowHandle));

        #endregion

        gameDataService.GameFullscreenChanged
            .Do(isFullscreen => this.Log().Debug("Game fullscreen: " + isFullscreen))
            .Subscribe(isFullscreen => stayTopSubj.OnNext(isFullscreen))
            .DisposeWith(_disposables);

        Loaded = ReactiveCommand.Create(() =>
        {
            windowDataService.InitMainWindowHandle(MainWindowHandle);
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
            Dpi = windowDataService.Dpi = newDpi;
            windowDataService.DpiOnNext(newDpi);

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

    [ObservableAsProperty]
    public bool ShowEdgeTouchMask { get; } = default;

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    public ReactiveCommand<double, Unit> DpiChanged { get; }

    public Interaction<Unit, Unit> ShowMainWindow { get; } = new();

    public Interaction<Unit, Unit> HideMainWindow { get; } = new();

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
