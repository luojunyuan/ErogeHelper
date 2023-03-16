using System.Reactive.Disposables;
using ErogeHelper.Common.Definitions;
using ReactiveUI;

namespace ErogeHelper.ViewModel.Preference;

public class AboutViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string UrlPathSegment => PreferencePageTag.About;

    public ViewModelActivator Activator => new();

    public AboutViewModel()
    {
        var disposables = new CompositeDisposable();

        this.WhenActivated(d => d(disposables));
    }
}
