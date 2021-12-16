using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

public interface IMeCabService
{
    bool Loaded { get; }

    void CreateTagger();

    List<MeCabWord> GenerateMeCabWords(string sentence);
}
