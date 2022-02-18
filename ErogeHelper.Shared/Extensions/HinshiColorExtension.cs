using System.Drawing;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Shared.Extensions;

public static class HinshiColorExtension
{
    public static JapanesePartOfSpeech ToHinshi(this string partOfSpeech)
    {
        return partOfSpeech switch
        {
            "名詞" => JapanesePartOfSpeech.Noun,
            "動詞" => JapanesePartOfSpeech.Verb,
            "形容詞" => JapanesePartOfSpeech.Adjective,
            "副詞" => JapanesePartOfSpeech.Adverb,
            "助詞" => JapanesePartOfSpeech.Auxiliary,
            "助動詞" => JapanesePartOfSpeech.AuxiliaryVerb,
            "感動詞" => JapanesePartOfSpeech.Interjection,
            "形状詞" => JapanesePartOfSpeech.Form,
            "代名詞" => JapanesePartOfSpeech.Pronoun,
            "連体詞" => JapanesePartOfSpeech.Conjunction,
            "接尾辞" => JapanesePartOfSpeech.Suffix,
            "補助記号" => JapanesePartOfSpeech.Mark,
            _ => JapanesePartOfSpeech.Undefined
        };
    }

    /// <returns>Can only use LightGreen Green Pink three colors</returns>
    public static Color ToColor(this JapanesePartOfSpeech partOfSpeech)
    {
        return partOfSpeech switch
        {
            JapanesePartOfSpeech.Noun or JapanesePartOfSpeech.Pronoun or JapanesePartOfSpeech.Kanji
                => Color.LightGreen,
            JapanesePartOfSpeech.Verb or JapanesePartOfSpeech.AuxiliaryVerb or JapanesePartOfSpeech.Adverb
                => Color.Green,
            JapanesePartOfSpeech.Adjective or JapanesePartOfSpeech.Interjection or JapanesePartOfSpeech.Form or
            JapanesePartOfSpeech.Conjunction or JapanesePartOfSpeech.Suffix
                => Color.Pink,
            _
                => Color.Transparent, // Hinshi.助詞
        };
    }
}
