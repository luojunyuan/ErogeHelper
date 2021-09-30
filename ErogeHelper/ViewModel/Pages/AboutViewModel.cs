using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using Splat;

namespace ErogeHelper.ViewModel.Pages
{
    public class AboutViewModel : ReactiveObject, IRoutableViewModel, IEnableLogger
    {
        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.About;

        public AboutViewModel(IPreferenceScreen? hostScreen = null)
        {
            HostScreen = hostScreen ?? DependencyInject.GetService<IPreferenceScreen>();
        }
    }
}
