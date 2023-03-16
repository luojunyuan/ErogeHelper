using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.MainGame;

public class MainGameViewModel : ReactiveObject, IEnableLogger
{
    private const int GameFullscreenStatusRefreshTime = 200;

    public MainGameViewModel(
        IEHConfigRepository? ehConfigRepository = null,
        IGameWindowHooker? gameWindowHooker = null,
        IGameInfoRepository? gameInfoRepository = null)
    {
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        gameWindowHooker.GamePosUpdated
            .Subscribe(pos =>
            {
                Height = pos.Height / State.Dpi;
                Width = pos.Width / State.Dpi;
                Left = pos.Left / State.Dpi;
                Top = pos.Top / State.Dpi;
            });

        gameWindowHooker.WhenViewOperated
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(operation =>
            {
                switch (operation)
                {
                    case ViewOperation.Show:
                        ShowMainWindow.Handle(Unit.Default).Subscribe();
                        break;
                    case ViewOperation.Hide:
                        HideMainWindow.Handle(Unit.Default).Subscribe();
                        break;
                }
            });

        #region BringWindowToTop Subject

        var stayTopSubj = new Subject<bool>();

        var interval = Observable
            .Interval(TimeSpan.FromMilliseconds(GameFullscreenStatusRefreshTime))
            .TakeUntil(stayTopSubj.Where(on => !on));

        stayTopSubj
            .DistinctUntilChanged()
            .Where(on => on)
            .SelectMany(interval)
            .Subscribe(_ => User32.BringWindowToTop(State.MainGameWindowHandle));

        #endregion

        State.GameFullscreenChanged
            .Skip(1) // Skip first time cause its a BehaviorSubject
            .Do(isFullscreen => this.Log().Info("Game fullscreen: " + isFullscreen))
            .Do(_ => User32.BringWindowToTop(State.MainGameWindowHandle))
            .Subscribe(isFullscreen => stayTopSubj.OnNext(isFullscreen));

        State.GameFullscreenChanged
            .CombineLatest(gameInfoRepository.WhenAnyValue(x => x.IsLoseFocus))
            .Select(pair => pair.First && pair.Second)
            .Subscribe(v =>
            {
                HwndTools.WindowLostFocus(State.MainGameWindowHandle, v);
                HwndTools.WindowLostFocus(State.TextWindowHandle, v);
            });

        // HACK: EH receive the dpi changed event twice if game on second screen when starting
        State.DpiChanged
            .SelectMany(_ => Observable.Start(() => Unit.Default)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ => gameWindowHooker.InvokeUpdatePosition()))
            .Subscribe();

        Loaded = ReactiveCommand.Create<Unit, Unit>(_ =>
        {
            // The first time position get invoked
            gameWindowHooker.InvokeUpdatePosition();
            stayTopSubj.OnNext(State.IsFullscreen);
            State.GameFullscreenChanged
                .CombineLatest(ehConfigRepository.WhenAnyValue(x => x.UseEdgeTouchMask))
                .Select(pair => pair.First && pair.Second)
                .Subscribe(v => AddEdgeMask.Handle(v).Subscribe());
            return Unit.Default;
        });
    }

    [Reactive]
    public double Height { get; set; }

    [Reactive]
    public double Width { get; set; }

    [Reactive]
    public double Left { get; set; }

    [Reactive]
    public double Top { get; set; }

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    public Interaction<Unit, Unit> ShowMainWindow { get; } = new();

    public Interaction<Unit, Unit> HideMainWindow { get; } = new();

    public Interaction<bool, Unit> AddEdgeMask { get; } = new();
}
