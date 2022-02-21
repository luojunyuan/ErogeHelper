using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class TTSViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => PageTag.TTS;

    public IScreen HostScreen => throw new NotImplementedException();
}
