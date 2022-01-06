using System.Windows;
using ErogeHelper.Shared.Structs;
using Windows.Globalization;

namespace ErogeHelper.Platform;

internal static class WinRTHelper
{
    /// <param name="sentence">The maximum length of the sentence is 100 characters</param>
    public static IEnumerable<MeCabWord> JapaneseAnalyzer(string sentence)
    {
        // Seems like must be called in main thread
        var (phonemes, count) = Application.Current.Dispatcher.Invoke(() =>
        {
            var phonemes = JapanesePhoneticAnalyzer.GetWords(sentence);
            return (phonemes, phonemes.Count);
        });

        for (var i = 0; i < count; i++)
        {
            var isKanji = WanaKanaNet.WanaKana.IsKanji(phonemes[i].DisplayText);
            if (!isKanji)
            {
                var stripedWord = WanaKanaNet.WanaKana.StripOkurigana(phonemes[i].DisplayText);
                isKanji = WanaKanaNet.WanaKana.IsKanji(stripedWord);
            }

            yield return new MeCabWord()
            {
                Word = phonemes[i].DisplayText,
                Kana = phonemes[i].YomiText,
                WordIsKanji = isKanji
            };
        }
    }
}
