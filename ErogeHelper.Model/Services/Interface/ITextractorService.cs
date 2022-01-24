using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

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
    void InjectProcesses(IGameDataService? gameDataService = null);

    /// <param name="hookcode">if there is : and suffix with exe, the executable name must be file name. 
    /// How about .log?</param>
    void InsertHook(string hookcode);

    void SearchRCode(string text);

    List<string> GetConsoleOutputInfo();

    void ReAttachProcesses();

    void RemoveHook(long address);

    void RemoveUselessHooks();

    void AddClipboardText(string text);
}
