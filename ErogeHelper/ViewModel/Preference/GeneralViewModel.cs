using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Function;
using ErogeHelper.Model.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class GeneralViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string UrlPathSegment => PreferencePageTag.General;

    public GeneralViewModel(IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        TouchSize = ehConfigRepository.AssistiveTouchBig ? TouchSizeSource[1] : TouchSizeSource[0];
        this.WhenAnyValue(x => x.TouchSize)
            .Skip(1)
            .Select(str => !str.Equals(TouchSizeSource[0]))
            .Subscribe(v => ehConfigRepository.AssistiveTouchBig = v);

        StartupInject = ehConfigRepository.StartupInjectProcess;
        this.WhenAnyValue(x => x.StartupInject)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.StartupInjectProcess = v);

        ShowTextWindow = ehConfigRepository.ShowTextWindow;
        this.WhenAnyValue(x => x.ShowTextWindow)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.ShowTextWindow = v);
        

        UseDPIDpiCompatibility = ehConfigRepository.DPICompatibilityByApplication;
        this.WhenAnyValue(x => x.UseDPIDpiCompatibility)
            .Skip(1)
            .Throttle(EHContext.UserConfigOperationDelay)
            .DistinctUntilChanged()
            .Subscribe(v => ehConfigRepository.DPICompatibilityByApplication = v);

        UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;
        this.WhenAnyValue(x => x.UseEdgeTouchMask)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseEdgeTouchMask = v);



        UseMagSmoothing = ehConfigRepository.MagSmoothing;
        this.WhenAnyValue(x => x.UseMagSmoothing)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.MagSmoothing = v);

        MagDataString = ehConfigRepository.MagSourceInputString;
        this.WhenAnyValue(x => x.MagDataString)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.MagSourceInputString = v);
    }


    [Reactive]
    public ReadOnlyObservableCollection<string> TouchSizeSource { get; set; }
        = new(new ObservableCollection<string>() { "Normal", "Bigger" });

    [Reactive]
    public string TouchSize { get; set; }

    [Reactive]
    public bool ShowTextWindow { get; set; }

    [Reactive]
    public bool UseEdgeTouchMask { get; set; }

    [Reactive]
    public bool StartupInject { get; set; }

    [Reactive]
    public bool UseDPIDpiCompatibility { get; set; }


    [Reactive]
    public bool UseMagSmoothing { get; set; }
    [Reactive]
    public string MagDataString { get; set; }
}
