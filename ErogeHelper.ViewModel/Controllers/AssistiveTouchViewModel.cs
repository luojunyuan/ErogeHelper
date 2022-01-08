using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Controllers;

public class AssistiveTouchViewModel : ReactiveObject, IActivatableViewModel, IEnableLogger
{
    public bool IsTouchBigSize { get; }
    private readonly Subject<bool> _useBigSizeSubj = new();
    public IObservable<bool> UseBigSize => _useBigSizeSubj;

    public AssistiveTouchPosition AssistiveTouchPosition { get; }
    public Subject<AssistiveTouchPosition> AssistiveTouchPositionChanged { get; } = new();

    public ViewModelActivator Activator => new();

    private readonly IEHConfigRepository _ehConfigRepository;
    public AssistiveTouchViewModel(
        IEHConfigRepository? ehConfigDataService = null,
        IGameDataService? gameDataService = null,
        IGameInfoRepository? ehDbRepository = null,
        ITouchConversionHooker? touchConversionHooker = null,
        IGameWindowHooker? gameWindowHooker = null,
        TouchToolBoxViewModel? touchToolBoxViewModel = null,
        IWindowDataService? windowDataService = null)
    {
        _ehConfigRepository = ehConfigDataService ?? DependencyResolver.GetService<IEHConfigRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        touchConversionHooker ??= DependencyResolver.GetService<ITouchConversionHooker>();
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        touchToolBoxViewModel ??= DependencyResolver.GetService<TouchToolBoxViewModel>();
        windowDataService ??= DependencyResolver.GetService<IWindowDataService>();

        AssistiveTouchPosition = JsonSerializer.Deserialize<AssistiveTouchPosition>
            (_ehConfigRepository.AssistiveTouchPosition) ?? AssistiveTouchPosition.Default;
        var disposables = new CompositeDisposable();

#if !DEBUG // https://stackoverflow.com/questions/63723996/mouse-freezing-lagging-when-hit-breakpoint
        touchConversionHooker.Init();
        touchConversionHooker.DisposeWith(disposables);
#endif
        ShowAssistiveTouch = true;
        SwitchFullScreenIcon = SymbolName.FullScreen;
        SwitchFullScreenToolTip = Strings.GameView_SwitchFullScreen;
        LoseFocusEnable = ehDbRepository.GameInfo.IsLoseFocus;
        TouchBoxEnable = _ehConfigRepository.UseTouchToolBox;
        IsTouchToMouse = ehDbRepository.GameInfo.IsEnableTouchToMouse;

        IsTouchBigSize = _ehConfigRepository.UseBigAssistiveTouchSize;

        _ehConfigRepository.WhenAnyValue(x => x.UseBigAssistiveTouchSize)
            .Skip(1)
            .Subscribe(useBigSize => _useBigSizeSubj.OnNext(useBigSize))
            .DisposeWith(disposables);

        AssistiveTouchPositionChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(pos => _ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(pos))
            .DisposeWith(disposables);

        // Flyout bar commands implements
        gameDataService
            .GameFullscreenChanged
            .Subscribe(isFullscreen =>
            {
                if (isFullscreen)
                {
                    SwitchFullScreenIcon = SymbolName.BackToWindow;
                    SwitchFullScreenToolTip = Strings.GameView_SwitchWindow;
                }
                else
                {
                    SwitchFullScreenIcon = SymbolName.FullScreen;
                    SwitchFullScreenToolTip = Strings.GameView_SwitchFullScreen;
                }
            })
            .DisposeWith(disposables);

        this.WhenAnyValue(x => x.LoseFocusEnable)
            .Skip(1)
            .Subscribe(v =>
            {
                HwndTools.WindowLostFocus(windowDataService.MainWindowHandle, v);
                HwndTools.WindowLostFocus(windowDataService.TextWindowHandle ?? IntPtr.Zero, v);
                ehDbRepository.UpdateLostFocusStatus(v);
            });

        this.WhenAnyValue(x => x.LoseFocusEnable)
            .ToPropertyEx(this, x => x.TouchBoxSwitcherVisible);

        this.WhenAnyValue(x => x.TouchBoxEnable)
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(v => _ehConfigRepository.UseTouchToolBox = v);
        this.WhenAnyValue(x => x.LoseFocusEnable, x => x.TouchBoxEnable, (a, b) => a && b)
            .ToPropertyEx(touchToolBoxViewModel, x => x.TouchToolBoxVisible)
            .DisposeWith(disposables);

        this.WhenAnyValue(x => x.IsTouchToMouse)
            .Skip(1)
            .Subscribe(v =>
            {
                touchConversionHooker.Enable = v;
                ehDbRepository.UpdateTouchEnable(v);
            });

        SwitchFullScreen = ReactiveCommand.Create(() =>
            User32.BringWindowToTop(gameDataService.MainProcess.MainWindowHandle));

        this.WhenActivated(d => disposables.DisposeWith(d));
    }

    [Reactive]
    public bool ShowAssistiveTouch { get; set; }

    [Reactive]
    public bool LoseFocusEnable { get; set; }

    [ObservableAsProperty]
    public bool TouchBoxSwitcherVisible { get; } = default;

    [Reactive]
    public bool TouchBoxEnable { get; set; }

    [Reactive]
    public bool IsTouchToMouse { get; set; }

    [Reactive]
    public SymbolName SwitchFullScreenIcon { get; set; }

    [Reactive]
    public string SwitchFullScreenToolTip { get; set; }

    public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }
}
