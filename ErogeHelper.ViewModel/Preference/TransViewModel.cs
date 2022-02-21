using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class TransViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => PageTag.Trans;

    public IScreen HostScreen => throw new NotImplementedException();
}
