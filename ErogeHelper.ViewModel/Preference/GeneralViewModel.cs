using System.Reactive.Linq;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class GeneralViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string? UrlPathSegment => PageTag.General;

    public GeneralViewModel(
        IWindowDataService? windowDataService = null,
        IGameInfoRepository? gameInfoRepository = null,
        ITouchConversionHooker? touchConversionHooker = null,
        IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        windowDataService ??= DependencyResolver.GetService<IWindowDataService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        touchConversionHooker ??= DependencyResolver.GetService<ITouchConversionHooker>();

        LoseFocusEnable = gameInfoRepository.GameInfo.IsLoseFocus;
        this.WhenAnyValue(x => x.LoseFocusEnable)
            .Skip(1)
            .Subscribe(v =>
            {
                HwndTools.WindowLostFocus(windowDataService.MainWindowHandle, v);
                HwndTools.WindowLostFocus(windowDataService.TextWindowHandle ?? new(), v);
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

        UseBigSizeAssistiveTouch = ehConfigRepository.UseBigAssistiveTouchSize;
        this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseBigAssistiveTouchSize = v);

        StartupInject = ehConfigRepository.InjectProcessByDefalut;
        this.WhenAnyValue(x => x.StartupInject)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.InjectProcessByDefalut = v);

        HideTextWindow = ehConfigRepository.HideTextWindow;
        this.WhenAnyValue(x => x.HideTextWindow)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.HideTextWindow = v);

        UseDPIDpiCompatibility = ehConfigRepository.DPICompatibilityByApplication;
        this.WhenAnyValue(x => x.UseDPIDpiCompatibility)
            .Skip(1)
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .DistinctUntilChanged()
            .Subscribe(v => ehConfigRepository.DPICompatibilityByApplication = v);

        UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;
        this.WhenAnyValue(x => x.UseEdgeTouchMask)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseEdgeTouchMask = v);
    }

    [Reactive]
    public bool LoseFocusEnable { get; set; }

    [Reactive]
    public bool TouchToMouseEnable { get; set; }

    [Reactive]
    public bool UseBigSizeAssistiveTouch { get; set; }

    [Reactive]
    public bool StartupInject { get; set; }

    [Reactive]
    public bool HideTextWindow { get; set; }

    [Reactive]
    public bool UseDPIDpiCompatibility { get; set; }

    [Reactive]
    public bool UseEdgeTouchMask { get; set; }
}
