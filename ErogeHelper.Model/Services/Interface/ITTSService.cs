using System.Reactive;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Model.Services.Interface;

public interface ITTSService
{
    List<string> GetAllVoice(TransLanguage language = TransLanguage.日本語);

    void PlayAudio(string sentence, string voiceName);
}
