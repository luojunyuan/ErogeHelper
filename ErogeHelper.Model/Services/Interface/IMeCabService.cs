using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

public interface IMeCabService : IDisposable
{
    bool CanLoadMeCab { get; }

    void LoadMeCabTagger();

    List<MeCabWord> GenerateMeCabWords(string sentence);
}
