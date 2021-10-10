using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace ErogeHelper.ViewModel.Pages
{
    public class GeneralViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.General;

        public GeneralViewModel(
            IScreen? hostScreen = null,
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigRepository = null,
            MainGameViewModel? mainGameViewModel = null)
        {
            mainWindowDataService ??= DependencyResolver.GetService<IMainWindowDataService>();
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();
            mainGameViewModel ??= DependencyResolver.GetService<MainGameViewModel>();

            HostScreen = hostScreen!;

            UseBigSizeAssistiveTouch = ehConfigRepository.UseBigAssistiveTouchSize;
            this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
                .Skip(1)
                .Subscribe(v =>
                {
                    ehConfigRepository.UseBigAssistiveTouchSize = v;
                    mainWindowDataService.AssistiveTouchBigSizeSubj.OnNext(v);
                });

            UseDPIDpiCompatibility = ehConfigRepository.DPIByApplication;
            this.WhenAnyValue(x => x.UseDPIDpiCompatibility)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.UserOperationDelay))
                .DistinctUntilChanged()
                .Subscribe(v => ehConfigRepository.DPIByApplication = v);

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;
            this.WhenAnyValue(x => x.UseEdgeTouchMask)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.UserOperationDelay))
                .DistinctUntilChanged()
                // XXX: Infact we don't need use another scheduler in such perioed lower than 10ms
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Do(v => ehConfigRepository.UseEdgeTouchMask = v)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(v => mainGameViewModel.UseEdgeTouchMask = v);
        }

        [Reactive]
        public bool UseBigSizeAssistiveTouch { get; set; }

        [Reactive]
        public bool UseDPIDpiCompatibility { get; set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }
    }
}
