using System.Diagnostics;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

public interface ITextractorService
{
    IObservable<HookParam> Data { get; }

    IObservable<HookParam> SelectedData { get; }

    TextractorSetting Setting { get; set; }

    bool Injected { get; }

    /// <summary>
    /// Inject hooks into processes, also initialize Textractor service. This should be called only once
    /// </summary>
    /// <param name="gameProcesses"></param>
    /// <param name="setting">Textractor init callback functions depends on some parameters</param>
    void InjectProcesses(List<Process> gameProcesses);

    void InsertHook(string hookcode);

    void SearchRCode(string text);

    List<string> GetConsoleOutputInfo();

    void ReAttachProcesses();

    void RemoveHook(long address);

    void RemoveUselessHooks();
}
