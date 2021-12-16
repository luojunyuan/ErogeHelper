using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Shared.Structs;

/// <param name="PartOfSpeech">Only use in Mecab dictionary</param>
/// <param name="WordIsAlreadyHiragana">Only use in WinRT Japanese linguistic analizer</param>
public readonly record struct MeCabWord(
    string Word,
    string Kana,
    JapanesePartOfSpeech PartOfSpeech,
    bool WordIsKanji);
