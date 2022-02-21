using System.Reactive.Linq;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Enums;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace ErogeHelper.Platform.WinRTHelper;

internal class TTSWinRT : ITTSService
{
    private readonly MediaPlayer Player = new();

    public List<string> GetAllVoice(TransLanguage language = TransLanguage.日本語)
    {
        if (language == TransLanguage.日本語)
        {
            return SpeechSynthesizer.AllVoices
                .Where(v => v.Language == "ja-JP")
                .Select(v => v.DisplayName).ToList();
        }

        return SpeechSynthesizer.AllVoices.Select(v => v.DisplayName).ToList();
    }

    public void PlayAudio(string sentence, string voiceName)
    {
        var synthesizer = new SpeechSynthesizer
        {
            Voice = SpeechSynthesizer.AllVoices
                .Where(v => v.DisplayName.Equals(voiceName, StringComparison.Ordinal))
                .First()
        };

        Observable.FromAsync(async () => await synthesizer.SynthesizeTextToStreamAsync(sentence))
            .Select(stream => MediaSource.CreateFromStream(stream, stream.ContentType))
            .Subscribe(source =>
            {
                Player.Source = source;
                Player.Play();
            });
    }
}
