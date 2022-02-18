using System.Windows;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Structs;
using WanaKanaNet;
using Windows.Globalization;

namespace ErogeHelper.Platform;

internal static class WinRTHelper
{
    private static IEnumerable<string> Split(string str, int chunkSize) =>
        Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize));

    /// <param name="sentence">The maximum length of the sentence is 100 characters</param>
    public static IEnumerable<MeCabWord> JapaneseAnalyzer(string sentence)
    {
        // TODO: Fix Japanese words when length bigger than 100
        if (sentence.Length > 100)
        {
            sentence = sentence[..100];
        }
        // Seems like must be called in main thread
        var (phonemes, count) = Application.Current.Dispatcher.Invoke(() =>
        {
            var japanesePhonemes = JapanesePhoneticAnalyzer.GetWords(sentence);
            return (japanesePhonemes, japanesePhonemes.Count);
        });

        for (var i = 0; i < count; i++)
        {
            var stripedWord = WanaKana.StripOkurigana(phonemes[i].DisplayText);
            var isKanji = ContainKanji(stripedWord);

            yield return new MeCabWord()
            {
                Word = phonemes[i].DisplayText,
                Kana = phonemes[i].YomiText,
                PartOfSpeech = isKanji ? JapanesePartOfSpeech.Kanji : JapanesePartOfSpeech.Undefined,
                WordIsKanji = isKanji
            };
        }
    }

    private static bool ContainKanji(string input) => input.Any(c => '一' <= c && c <= '龯');
}
