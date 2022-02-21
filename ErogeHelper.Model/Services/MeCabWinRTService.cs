using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Structs;
using WanaKanaNet;

namespace ErogeHelper.Model.Services;

public class MeCabWinRTService : IMeCabService
{
    public static Func<string, IEnumerable<MeCabWord>> JapaneseAnalyzerCallback { private get; set; } = null!;

    public bool CanLoadMeCab => true;

    public void LoadMeCabTagger() { }

    public List<MeCabWord> GenerateMeCabWords(string sentence)
    {
        // phrase like すっご would break winrt Japanese analyzer
        var words = MeCabWordWinRTEnumerable(sentence).ToList();
        return words.Count == 0 ? new()
        {
            new() { Word = sentence, Kana = " ", WordIsKanji = false }
        } : words;
    }

    private readonly IEHConfigRepository _configRepository;

    public MeCabWinRTService(IEHConfigRepository? ehConfigRepository = null)
        => _configRepository = ehConfigRepository ?? DependencyResolver.GetService<IEHConfigRepository>();

    private IEnumerable<MeCabWord> MeCabWordWinRTEnumerable(string sentence)
    {
        foreach (var mecabWord in JapaneseAnalyzerCallback(sentence))
        {
            var kana = " "; // full-width space to force render it
            if (_configRepository.KanaRuby == KanaRuby.Romaji)
            {
                // all to romaji
                kana = WanaKana.ToRomaji(mecabWord.Kana);
            }
            else if (mecabWord.WordIsKanji)
            {
                // kanji or katakana to katakana
                kana = _configRepository.KanaRuby == KanaRuby.Katakana
                    ? WanaKana.ToKatakana(mecabWord.Kana)
                    : mecabWord.Kana; // hiragana by default
            }

            yield return mecabWord with { Kana = kana };
        }
    }

    public void Dispose() { }
}
