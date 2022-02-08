using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

public interface IMeCabService : IDisposable
{
    bool CanLoaded { get; }

    void LoadMeCabTagger();

    List<MeCabWord> GenerateMeCabWords(string sentence);
}
