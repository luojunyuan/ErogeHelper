using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace ErogeHelper.ViewModel.Pages
{
    public class GeneralViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTag.General;

        public GeneralViewModel(
            IScreen? hostScreen = null,
            IEhConfigRepository? ehConfigRepository = null)
        {
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();

            HostScreen = hostScreen!;

            UseBigSizeAssistiveTouch = ehConfigRepository.UseBigAssistiveTouchSize;
            this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
                .Skip(1)
                .Subscribe(v => ehConfigRepository.UseBigAssistiveTouchSize = v);

            UseDPIDpiCompatibility = ehConfigRepository.DPIByApplication;
            this.WhenAnyValue(x => x.UseDPIDpiCompatibility)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
                .DistinctUntilChanged()
                .Subscribe(v => ehConfigRepository.DPIByApplication = v);

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;
            this.WhenAnyValue(x => x.UseEdgeTouchMask)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
                .DistinctUntilChanged()
                .Subscribe(v => ehConfigRepository.UseEdgeTouchMask = v);
        }

        [Reactive]
        public bool UseBigSizeAssistiveTouch { get; set; }

        [Reactive]
        public bool UseDPIDpiCompatibility { get; set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }
    }
}
