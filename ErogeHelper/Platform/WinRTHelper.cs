using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using CommunityToolkit.WinUI.Notifications;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Structs;
using Splat;
using WanaKanaNet;
using Windows.Globalization;

namespace ErogeHelper.Platform;

internal static class JapaneseAnalyzerWinRT
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

internal class ToastManagementWinRT : IToastManagement
{
    public ToastManagementWinRT() =>
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            if (toastArgs.Argument.Length == 0)
            {
                LogHost.Default.Debug("Toast Clicked");
                return;
            }
            var toastArguments = ToastArguments.Parse(toastArgs.Argument);
            LogHost.Default.Debug(toastArguments.ToString());
        };

    public void Show(string mainText)
    {
        new ToastContentBuilder()
            .AddText(mainText)
            .Show(toast =>
            {
                toast.Group = "eh";
                toast.Tag = "eh";
                // ExpirationTime bugged with InvalidCastException in .Net5
                // ExpirationTime can not work and bugged with using
                // ToastNotificationManagerCompat.History.Clear() in .Net6
                //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            });

        Thread.Sleep(IToastManagement.ToastDurationTime);
        ToastNotificationManagerCompat.History.Clear();
    }

    public async Task ShowAsync(string mainText, Stopwatch toastLifetimeTimer)
    {
        new ToastContentBuilder()
            .AddText(mainText)
            .Show(toast =>
            {
                toast.Group = "eh";
                toast.Tag = "eh";
                // ExpirationTime bugged with InvalidCastException in .Net5
                // ExpirationTime can not work and bugged with using
                // ToastNotificationManagerCompat.History.Clear() in .Net6
                //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            });

        toastLifetimeTimer.Restart();
        await Task.Delay(IToastManagement.ToastDurationTime).ConfigureAwait(false);
        if (toastLifetimeTimer.ElapsedMilliseconds >= IToastManagement.ToastDurationTime)
        {
            ToastNotificationManagerCompat.History.Clear();
            toastLifetimeTimer.Stop();
        }
    }

    public void InAdminModeToastTip()
    {
        var current = WindowsIdentity.GetCurrent();
        var windowsPrincipal = new WindowsPrincipal(current);
        if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            return;

        Show("ErogeHelper is running in Admin");
    }
}
