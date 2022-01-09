using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Function;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Structs;
using Splat;

namespace ErogeHelper.Model.Services;

public class TextractorHost : ITextractorService, IEnableLogger
{
    private readonly Subject<HookParam> _dataSubj = new();
    private readonly Subject<HookParam> _selectedDataSubj = new();
    private readonly List<string> _consoleOutput = new();

    public IObservable<HookParam> Data => _dataSubj;

    public IObservable<HookParam> SelectedData => _selectedDataSubj;

    public TextractorSetting Setting { get; private set; } = null!;

    public bool Injected { get; private set; } = false;

    private IGameDataService? _gameDataService;
    private ReadOnlyCollection<Process> GameProcesses => _gameDataService!.GameProcesses.AsReadOnly();

    /// <inheritdoc />
    public void InjectProcesses(IGameDataService? gameDataService = null)
    {
        if (Injected)
        {
            return;
        }
        Injected = true;

        _gameDataService = gameDataService;

        // Current texthook.dll version 4.15
        var textractorPath = Directory.GetCurrentDirectory() + @"\libs\texthost.dll";
        if (!File.Exists(textractorPath))
            throw new FileNotFoundException(textractorPath);

        _createThread = CreateThreadHandle;
        _output = OutputHandle;
        _removeThread = RemoveThreadHandle;
        _callback = OnConnectCallBackHandle;

        // TODO: https://github.com/ErogeHelper/ErogeHelper/issues/11
        _ = TextHostDll.TextHostInit(_callback, _ => { }, _createThread, _removeThread, _output);

        foreach (Process p in GameProcesses)
        {
            _ = TextHostDll.InjectProcess((uint)p.Id);
            this.Log().Debug($"attach to PID {p.Id}.");
        }
    }

    public void InsertHook(string hookcode)
    {
        if (hookcode.StartsWith('/'))
            hookcode = hookcode[1..];

        string engineName;
        if (hookcode.StartsWith('R'))
        {
            // EngineName = "READ";
            if (_threadHandleDict.Any(hcodeItem => hookcode.Equals(hcodeItem.Value.HookCode)))
            {
                _dataSubj.OnNext(new HookParam
                {
                    Handle = 0,
                    Pid = 0,
                    Address = -1,
                    Ctx = -1,
                    Ctx2 = -1,
                    Name = "Console",
                    HookCode = "HB0@0",
                    Text = "ErogeHelper: The Read-Code has already insert"
                });
                return;
            }
        }
        else
        {
            // HCode
            engineName = hookcode[(hookcode.LastIndexOf(':') + 1)..];

            // Note: Re-insert the same code may result a high address context*
            if (_threadHandleDict.Any(hcodeItem =>
                engineName.Equals(hcodeItem.Value.Name) || hookcode.Equals(hcodeItem.Value.HookCode)))
            {
                _dataSubj.OnNext(new HookParam
                {
                    Handle = 0,
                    Pid = 0,
                    Address = -1,
                    Ctx = -1,
                    Ctx2 = -1,
                    Name = "Console",
                    HookCode = "HB0@0",
                    Text = "ErogeHelper: The Hook-Code has already insert"
                });
                return;
            }
        }

        foreach (Process p in GameProcesses)
        {
            _ = TextHostDll.InsertHook((uint)p.Id, hookcode);
            this.Log().Debug($"Try insert hook {hookcode} to PID {p.Id}");
        }
    }

    public void ReAttachProcesses() => throw new NotImplementedException();
    public void RemoveHook(long address) =>
        GameProcesses.ToList().ForEach(p => _ = TextHostDll.RemoveHook((uint)p.Id, (ulong)address));

    public void RemoveUselessHooks() => throw new NotImplementedException();

    public void SearchRCode(string text) =>
        GameProcesses.ToList().ForEach(p => _ = TextHostDll.SearchForText((uint)p.Id, text, 932));

    public void SetSetting(TextractorSetting setting) => Setting = setting;
    public List<string> GetConsoleOutputInfo() => _consoleOutput;

    #region TextHost Callback Implement

    private TextHostDll.OnOutputText? _output;
    private TextHostDll.ProcessCallback? _callback;
    private TextHostDll.OnCreateThread? _createThread;
    private TextHostDll.OnRemoveThread? _removeThread;

    private readonly Dictionary<long, HookParam> _threadHandleDict = new();

    private bool firstTimeInject = true;

    private void CreateThreadHandle(
        long threadId,
        uint processId,
        ulong address,
        ulong context,
        ulong subContext,
        string name,
        string hookcode)
    {
        if (Setting.HookCode != string.Empty && !Setting.HookCode.Equals(hookcode) && firstTimeInject)
        {
            GameProcesses.ToList().ForEach(proc =>
                _ = TextHostDll.RemoveHook((uint)proc.Id, address));
        }

        _threadHandleDict[threadId] = new HookParam
        {
            Handle = threadId,
            Pid = processId,
            Address = (long)address,
            Ctx = (long)context,
            Ctx2 = (long)subContext,
            Name = name,
            HookCode = hookcode
        };
    }

    private void OutputHandle(long threadId, string opData, uint length)
    {
        if (length > 500)
            return;

        var hp = _threadHandleDict[threadId] = _threadHandleDict[threadId] with { Text = opData };

        _dataSubj.OnNext(hp);

        if (threadId == 0)
        {
            _consoleOutput.Add(Shared.Utils.ConsoleI18N(hp.Text));
            return;
        }

        foreach (var hookSetting in Setting.HookSettings)
        {
            if (Setting.HookCode.Equals(hp.HookCode)
                && (hookSetting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && hookSetting.SubThreadContext == hp.Ctx2)
            {
                this.Log().Debug(hp.Text);
                _selectedDataSubj.OnNext(hp);
            }
            // XXX: hp.Name `Search` `Read` is different
            else if (Setting.HookCode.StartsWith('R')
                     && hp.Name.Equals("READ"))
            {
                this.Log().Debug(hp.Text);
                _selectedDataSubj.OnNext(hp);
            }
        }
    }

    private void RemoveThreadHandle(long threadId) { }

    /// <summary>
    /// Called when console thread created
    /// </summary>
    private void OnConnectCallBackHandle(uint processId)
    {
        if (Setting.IsUserHook)
        {
            InsertHook(Setting.HookCode);
        }
    }
    #endregion
}
