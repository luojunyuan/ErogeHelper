using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.ViewModel.Routing;
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
        private readonly IEhConfigRepository _ehConfigRepositoy;

        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.General;

        public GeneralViewModel(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            IPreferenceScreen? hostScreen = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            _ehConfigRepositoy = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
            HostScreen = hostScreen ?? DependencyInject.GetService<IPreferenceScreen>();

            UseBigSizeAssistiveTouch = _ehConfigRepositoy.UseBigAssistiveTouchSize;
            this.WhenAnyValue(x => x.UseBigSizeAssistiveTouch)
                .Skip(1)
                .Subscribe(v => ChangeAssistiveTouchSize(v));
        }

        [Reactive]
        public bool UseBigSizeAssistiveTouch { get; set; }

        private void ChangeAssistiveTouchSize(bool bigStyle)
        {
            _ehConfigRepositoy.UseBigAssistiveTouchSize = bigStyle;
            _mainWindowDataService.AssistiveTouchBigSizeSubject.OnNext(bigStyle);
        }
    }
}
