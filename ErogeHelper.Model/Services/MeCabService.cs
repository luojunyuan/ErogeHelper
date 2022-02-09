using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Extensions;
using ErogeHelper.Shared.Structs;
using MeCab;
using MeCab.Extension.UniDic;
using WanaKanaNet;

namespace ErogeHelper.Model.Services;

public class MeCabService : IMeCabService
{
    private MeCabTagger? _tagger;
    private readonly IEHConfigRepository _configRepository;

    public MeCabService(IEHConfigRepository? ehConfigRepository = null)
    {
        _configRepository = ehConfigRepository ?? DependencyResolver.GetService<IEHConfigRepository>();

        CanLoaded = Directory.Exists(EHContext.MeCabDicFolder);
    }

    public bool CanLoaded { get; private set; }

    public void LoadMeCabTagger()
    {
        _tagger = MeCabTagger.Create(new MeCabParam
        {
            DicDir = EHContext.MeCabDicFolder
        });

        CanLoaded = true;
    }

    public List<MeCabWord> GenerateMeCabWords(string sentence) => MeCabWordUniDicEnumerable(sentence).ToList();

    private IEnumerable<MeCabWord> MeCabWordUniDicEnumerable(string sentence)
    {
        ArgumentNullException.ThrowIfNull(_tagger, nameof(_tagger));

        foreach (var node in _tagger.ParseToNodes(sentence))
        {
            if (node.CharType <= 0)
                continue;

            var hinshi = (node.GetPos1() ?? string.Empty).ToHinshi();
            var kana = " "; // full-width space to force render it
            if ((node.GetGoshu() ?? string.Empty).Equals("外", StringComparison.Ordinal))
            {
                // Katakana source form(外来語) like english supplied by unidic
                kana = (node.GetLemma() ?? " ").Split('-')[^1];
            }
            else if (_configRepository.KanaRuby == KanaRuby.Romaji)
            {
                // all to romaji
                kana = WanaKana.ToRomaji(node.GetPron() ?? " ");
            }
            else if (!WanaKana.IsKana(node.Surface) && hinshi != JapanesePartOfSpeech.Mark)
            {
                // kanji to kana
                kana = _configRepository.KanaRuby == KanaRuby.Hiragana
                    ? WanaKana.ToHiragana(node.GetPron() ?? " ")
                    : node.GetPron() ?? " "; // katakana by default
            }

            yield return new MeCabWord
            {
                Word = node.Surface,
                Kana = kana,
                PartOfSpeech = hinshi
            };
        }
    }

    public void Dispose()
    {
        _tagger?.Dispose();
        _tagger = null;
    }
}
