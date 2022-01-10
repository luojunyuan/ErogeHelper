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
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.MainGame;

public class MainGameViewModel : ReactiveObject, IDisposable
{
    public void UpdateDpi(double dpi) => _windowDataService.UpdateDpi(dpi);
    public void InitMainWindowHandle(HWND handle) => _windowDataService.InitMainWindowHandle(handle);

    public AssistiveTouchViewModel? AssistiveTouchViewModel { get; }

    private readonly IWindowDataService _windowDataService;

    public MainGameViewModel(
        AssistiveTouchViewModel? assistiveTouchViewModel = null,
        IEHConfigRepository? ehConfigRepository = null,
        IGameWindowHooker? gameWindowHooker = null,
        IGameInfoRepository? ehDbRepository = null,
        IGameDataService? gameDataService = null,
        IWindowDataService? windowDataService = null)
    {
        //AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyResolver.GetService<AssistiveTouchViewModel>();
        AssistiveTouchViewModel = assistiveTouchViewModel;
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        _windowDataService = windowDataService ?? DependencyResolver.GetService<IWindowDataService>();

        ehConfigRepository.WhenAnyValue(x => x.UseEdgeTouchMask)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.ShowEdgeTouchMask)
            .DisposeWith(_disposables);

        //this.WhenAnyValue(x => x.Menu.LoseFocusEnable, x => x.Menu.TouchBoxEnable, (a, b) => a && b)
        Observable.Return(false)
            .ToPropertyEx(this, x => x.TouchToolBoxVisible)
            .DisposeWith(_disposables);

        gameWindowHooker.GamePosUpdated
            .Subscribe(pos =>
            {
                Height = pos.Height / _windowDataService.Dpi;
                Width = pos.Width / _windowDataService.Dpi;
                Left = pos.Left / _windowDataService.Dpi;
                Top = pos.Top / _windowDataService.Dpi;
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
            .Where(_ => !_windowDataService.MainWindowHandle.IsNull)
            .Subscribe(_ => User32.BringWindowToTop(_windowDataService.MainWindowHandle));

        #endregion

        gameDataService.GameFullscreenChanged
            .Do(isFullscreen => this.Log().Debug("Game fullscreen: " + isFullscreen))
            // TODO: Turn off focus if exit fullscreen
            .Subscribe(isFullscreen => stayTopSubj.OnNext(isFullscreen))
            .DisposeWith(_disposables);

        Loaded = ReactiveCommand.Create(() =>
        {
            // The first time position get invoked
            gameWindowHooker.InvokeUpdatePosition();

            if (ehDbRepository.GameInfo.IsLoseFocus)
            {
                HwndTools.WindowLostFocus(_windowDataService.MainWindowHandle, ehDbRepository.GameInfo.IsLoseFocus);
            }
        });

        // HACK: EH may receive the dpi changed event faster than the game initialization
        // when starting for the first time 
        _windowDataService.DpiChanged
            .SelectMany(_ => Observable
                .Start(() => Unit.Default)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ => gameWindowHooker.InvokeUpdatePosition()))
            .Subscribe()
            .DisposeWith(_disposables);
    }

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

    [ObservableAsProperty]
    public bool TouchToolBoxVisible { get; }

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    public Interaction<Unit, Unit> ShowMainWindow { get; } = new();

    public Interaction<Unit, Unit> HideMainWindow { get; } = new();

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
