using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.MainGame.AssistiveTouchMenu;

public class MenuGameViewModel : ReactiveObject
{
    public MenuGameViewModel(
        ITouchConversionHooker? touchConversionHooker = null,
        IGameInfoRepository? gameInfoRepository = null,
        IGameDataService? gameDataService = null)
    {
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        touchConversionHooker ??= DependencyResolver.GetService<ITouchConversionHooker>();
        var disposables = new CompositeDisposable();

        SwitchFullScreenIcon = SymbolName.FullScreen;
        SwitchFullScreenText = Strings.AssistiveTouch_FullScreen;
        SwitchFullScreen = ReactiveCommand.Create(() =>
            User32.BringWindowToTop(gameDataService.MainProcess.MainWindowHandle));

        gameDataService.GameFullscreenChanged
            .Subscribe(isFullscreen =>
            {
                if (isFullscreen)
                {
                    SwitchFullScreenIcon = SymbolName.BackToWindow;
                    SwitchFullScreenText = Strings.AssistiveTouch_Window;
                }
                else
                {
                    SwitchFullScreenIcon = SymbolName.FullScreen;
                    SwitchFullScreenText = Strings.AssistiveTouch_FullScreen;
                }
            }).DisposeWith(disposables);

        CloseGame = ReactiveCommand.Create(() =>
        {
            User32.PostMessage(
                gameDataService.MainProcess.MainWindowHandle, 
                (uint)User32.WindowMessage.WM_SYSCOMMAND, 
                // ReSharper disable once RedundantArgumentDefaultValue
                (IntPtr)User32.SysCommand.SC_CLOSE);
            User32.BringWindowToTop(gameDataService.MainProcess.MainWindowHandle);
        });

        LoseFocusEnable = gameInfoRepository.GameInfo.IsLoseFocus;
        this.WhenAnyValue(x => x.LoseFocusEnable)
            .Skip(1)
            .Subscribe(v =>
            {
                if (v)
                {
                    Interactions.MessageBoxConfirm.Handle(Strings.AssistiveTouch_LoseFocusWarn).Subscribe();
                }
                HwndTools.WindowLostFocus(MainGameViewModel.MainGameWindowHandle, v);
                HwndTools.WindowLostFocus(TextViewModel.TextWindowHandle, v);
                gameInfoRepository.UpdateLostFocusStatus(v);
            });

        TouchToMouseEnable = gameInfoRepository.GameInfo.IsEnableTouchToMouse;
        this.WhenAnyValue(x => x.TouchToMouseEnable)
            .Skip(1)
            .Subscribe(v =>
            {
                touchConversionHooker.Enable = v;
                gameInfoRepository.UpdateTouchEnable(v);
            });
    }

    [Reactive]
    public SymbolName SwitchFullScreenIcon { get; set; }

    [Reactive]
    public string SwitchFullScreenText { get; set; }

    public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }

    public ReactiveCommand<Unit, Unit> CloseGame { get; }

    [Reactive]
    public bool LoseFocusEnable { get; set; }

    [Reactive]
    public bool TouchToMouseEnable { get; set; }
}
