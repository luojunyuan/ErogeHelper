using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Pages;

public class GeneralViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string? UrlPathSegment => PageTag.General;

    public GeneralViewModel(IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        UseBigSizeAssistiveTouch = ehConfigRepository.UseBigAssistiveTouchSize;
        this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseBigAssistiveTouchSize = v);

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
    public bool UseBigSizeAssistiveTouch { get; set; }

    [Reactive]
    public bool HideTextWindow { get; set; }

    [Reactive]
    public bool UseDPIDpiCompatibility { get; set; }

    [Reactive]
    public bool UseEdgeTouchMask { get; set; }
}
