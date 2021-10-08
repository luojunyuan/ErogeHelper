using ErogeHelper.Common.Contracts;
using ReactiveUI;
using Splat;

namespace ErogeHelper.ViewModel.Pages
{
    public class AboutViewModel : ReactiveObject, IRoutableViewModel, IEnableLogger
    {
        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.About;

        public AboutViewModel(IScreen? hostScreen = null)
        {
            HostScreen = hostScreen!;
        }
    }
}
