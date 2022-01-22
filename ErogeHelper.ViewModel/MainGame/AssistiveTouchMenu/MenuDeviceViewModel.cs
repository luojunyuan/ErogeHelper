using System.Reactive;
using System.Reactive.Disposables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.MainGame.AssistiveTouchMenu;

public class MenuDeviceViewModel : ReactiveObject
{
    public MenuDeviceViewModel(
        IGameDataService? gameDataService = null)
    {
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
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
    }


    [Reactive]
    public SymbolName SwitchFullScreenIcon { get; set; }

    [Reactive]
    public string SwitchFullScreenText { get; set; }

    public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }
}
