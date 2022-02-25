using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class TTSViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => PageTag.TTS;

    public IScreen HostScreen => throw new NotImplementedException();

    public TTSViewModel(ITTSService? ttsService = null, IEHConfigRepository? ehConfigRepository = null)
    {
        ttsService ??= DependencyResolver.GetService<ITTSService>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        _voices = new(ttsService.GetAllVoice());
        SelectedVoice = ehConfigRepository.TTSVoiceName;

        var canPlay = this.WhenAnyValue(x => x.SelectedVoice, v => !string.IsNullOrEmpty(v)); 
        Play = ReactiveCommand.Create(() => 
            ttsService.PlayAudio("すもももももももものうち", SelectedVoice), canPlay);

        this.WhenAnyValue(x => x.SelectedVoice)
            .Skip(1)
            .WhereNotNull()
            .Subscribe(v => ehConfigRepository.TTSVoiceName = v);
    }

    private readonly ReadOnlyCollection<string> _voices;
    public ReadOnlyCollection<string> Voices => _voices;

    [Reactive]
    public string? SelectedVoice { get; set; }

    public ReactiveCommand<Unit, Unit> Play { get; }
}
