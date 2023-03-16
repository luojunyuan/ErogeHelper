using ErogeHelper.Common.Entities;

namespace ErogeHelper.Model.Services;

public interface ITextractorService
{
    IObservable<HookParam> Data { get; }

    IObservable<HookParam> SelectedData { get; }

    void SetSetting(TextractorSetting setting);
    TextractorSetting Setting { get; }

    bool Injected { get; }

    /// <summary>
    /// Inject hooks into processes, also initialize Textractor service. This should be called only once
    /// </summary>
    void InjectProcesses();

    string GetConsoleOutputInfo();

    /// <param name="hookcode"></param>
    void InsertHook(string hookcode);

    void SearchRCode(string text);

    ValueTask ReAttachProcesses();

    bool FullSupport { get; }
    void RemoveHook(long address);

    void AddClipboardText(string text);

    // UNDONE: Write TextractorConfig.txt, for Unity game { string:Substring (int,int) }
}
