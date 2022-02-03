using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.MainGame;

public class MainGameViewModel : ReactiveObject, IDisposable
{
    private const int GameFullscreenStatusRefreshTime = 200;

    [Reactive]
    public DanmakuCanvasViewModel? DanmakuCanvasViewModel { get; private set; }

    public MainGameViewModel(
        IEHConfigRepository? ehConfigRepository = null,
        IGameWindowHooker? gameWindowHooker = null,
        IGameInfoRepository? ehGameInfoRepository = null,
        IGameDataService? gameDataService = null,
        IWindowDataService? windowDataService = null)
    {
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        ehGameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        windowDataService ??= DependencyResolver.GetService<IWindowDataService>();
        if (ehConfigRepository.UseDanmaku)
        {
            DanmakuCanvasViewModel = DependencyResolver.GetService<DanmakuCanvasViewModel>();
        }

        ehConfigRepository.WhenAnyValue(x => x.UseEdgeTouchMask)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.ShowEdgeTouchMask)
            .DisposeWith(_disposables);

        ehConfigRepository.WhenAnyValue(x => x.UseDanmaku)
            .Select(v => v ? DependencyResolver.GetService<DanmakuCanvasViewModel>() : null)
            .Subscribe(vm => DanmakuCanvasViewModel = vm)
            .DisposeWith(_disposables);

        gameWindowHooker.GamePosUpdated
            .Subscribe(pos =>
            {
                Height = pos.Height / State.Dpi;
                Width = pos.Width / State.Dpi;
                Left = pos.Left / State.Dpi;
                Top = pos.Top / State.Dpi;
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
            .Interval(TimeSpan.FromMilliseconds(GameFullscreenStatusRefreshTime))
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
            // QUESTION: May need re-locate button position
            .Subscribe(isFullscreen => stayTopSubj.OnNext(isFullscreen))
            .DisposeWith(_disposables);

        Loaded = ReactiveCommand.Create<HWND, Unit>(handle =>
        {
            // The first time position get invoked
            gameWindowHooker.InvokeUpdatePosition();

            windowDataService.InitMainWindowHandle(handle);
            HwndTools.WindowLostFocus(handle, ehGameInfoRepository.GameInfo.IsLoseFocus);
            return Unit.Default;
        });

        // HACK: EH receive the dpi changed event twice if game on second screen when starting
        State.DpiChanged
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

    public ReactiveCommand<HWND, Unit> Loaded { get; }

    public Interaction<Unit, Unit> ShowMainWindow { get; } = new();

    public Interaction<Unit, Unit> HideMainWindow { get; } = new();


    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
