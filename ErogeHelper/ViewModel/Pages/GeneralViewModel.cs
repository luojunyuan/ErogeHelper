using ErogeHelper.Common;
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
        private readonly IMainWindowDataService _mainWindowDataService;
        private readonly IEhConfigRepository _ehConfigRepository;

        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.General;

        public GeneralViewModel(
            IScreen? hostScreen = null,
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            MainGameViewModel? mainGameViewModel = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyResolver.GetService<IMainWindowDataService>();
            _ehConfigRepository = ehConfigDataService ?? DependencyResolver.GetService<IEhConfigRepository>();
            mainGameViewModel ??= DependencyResolver.GetService<MainGameViewModel>();

            HostScreen = hostScreen!;

            // Learn WhenAnyValue first
            UseBigSizeAssistiveTouch = _ehConfigRepository.UseBigAssistiveTouchSize;
            this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
                .Skip(1)
                .Subscribe(v =>
                {
                    _ehConfigRepository.UseBigAssistiveTouchSize = v;
                    _mainWindowDataService.AssistiveTouchBigSizeSubject.OnNext(v);
                });
            UseDPIDpiCompatibility = _ehConfigRepository.DPIByApplication;
            this.WhenAnyValue(x => x.UseDPIDpiCompatibility)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.UserOperationDelay))
                .DistinctUntilChanged()
                .Subscribe(v => _ehConfigRepository.DPIByApplication = v);
            UseEdgeTouchMask = _ehConfigRepository.UseEdgeTouchMask;
            this.WhenAnyValue(x => x.UseEdgeTouchMask)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.UserOperationDelay))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(v =>
                {
                    mainGameViewModel.UseEdgeTouchMask = v;
                    _ehConfigRepository.UseEdgeTouchMask = v;
                });
        }

        [Reactive]
        public bool UseBigSizeAssistiveTouch { get; set; }

        [Reactive]
        public bool UseDPIDpiCompatibility { get; set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }
    }
}
