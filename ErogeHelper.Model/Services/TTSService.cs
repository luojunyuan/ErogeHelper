using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Model.Services;

public class TTSService : ITTSService
{
    // https://stackoverflow.com/questions/60618283/where-is-the-sayaka-voice-in-speech-api-onecore
    public List<string> GetAllVoice(TransLanguage language) => throw new NotImplementedException();
    public void PlayAudio(string sentence, string voiceName) => throw new NotImplementedException();
}
