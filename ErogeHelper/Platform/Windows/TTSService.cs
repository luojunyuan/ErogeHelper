using System.Globalization;
using System.Reactive.Linq;
using System.Speech.Synthesis;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Platform.Windows;

public class TTSService : ITTSService
{
    // https://stackoverflow.com/questions/60618283/where-is-the-sayaka-voice-in-speech-api-onecore
    private readonly static CultureInfo Japanese = new("ja-JP");
    private readonly SpeechSynthesizer _speechSynthesizer = new();

    public List<string> GetAllVoice(TransLanguage language)
    {
        if (language == TransLanguage.日本語)
        {
            return _speechSynthesizer.GetInstalledVoices()
                .Where(v => v.VoiceInfo.Culture.Equals(Japanese))
                .Select(v => v.VoiceInfo.Name).ToList();
        }

        return _speechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();
    }

    public void PlayAudio(string sentence, string voiceName)
    {
        _speechSynthesizer.SelectVoice(voiceName);
        _speechSynthesizer.SpeakAsyncCancelAll();
        _speechSynthesizer.SpeakAsync(sentence);
    }
}
