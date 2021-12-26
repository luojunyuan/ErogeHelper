using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Structs;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services;

public class TextractorCli : ITextractorService, IEnableLogger
{
    private readonly Subject<HookParam> _dataSubj = new();
    public IObservable<HookParam> Data => _dataSubj;

    private readonly Subject<HookParam> _selectedDataSubj = new();
    public IObservable<HookParam> SelectedData => _selectedDataSubj;

    public TextractorSetting Setting { get; set; } = null!;

    private readonly List<string> _consoleOutput = new();
    public List<string> GetConsoleOutputInfo() => _consoleOutput;

    private Process _textractorCli = null!;

    private List<Process> _gameProcesses = null!;

    public bool Injected { get; private set; } = false;

    /// <inheritdoc />
    public void InjectProcesses(List<Process> gameProcesses)
    {
        _gameProcesses = gameProcesses;
        // CODESMELL: Operate _gameProcesses in IsX86Process() function
        bool isX86 = gameProcesses.ToList().Any(p => IsX86Process(p));

        var textractorCliPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "libs",
            isX86 ? "x86" : "x64",
            "cli",
            "TextractorCLI.exe");

        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.Unicode,
            StandardInputEncoding = new UnicodeEncoding(false, false),
            FileName = textractorCliPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _textractorCli = Process.Start(processStartInfo) ?? throw new InvalidOperationException();
        _textractorCli.OutputDataReceived += OutputDataRetrieveCallback;
        _textractorCli.BeginOutputReadLine();

        foreach (Process p in _gameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("attach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"attach to PID {p.Id}.");
        }

        if (Setting.IsUserHook)
        {
            InsertHook(Setting.HookCode);
        }

        Injected = true;
    }

    public void InsertHook(string hookcode)
    {
        foreach (Process p in _gameProcesses)
        {
            _textractorCli.StandardInput.WriteLine($"{hookcode} -P{p.Id}");
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"Insert code {hookcode} to PID {p.Id}.");
        }
    }

    public void ReAttachProcesses()
    {
        foreach (Process p in _gameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("detach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"detach to PID {p.Id}.");
        }
    }

    public void RemoveHook(long address) => throw new NotImplementedException();
    public void RemoveUselessHooks() => throw new NotImplementedException();
    public void SearchRCode(string text) => throw new NotImplementedException();

    private void OutputDataRetrieveCallback(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not string outputData ||
              !outputData.StartsWith('[') ||
              outputData.Length > 500)
        {
            return;
        }

        var regex = new Regex(@"\[(?<time>.*?):(?<pid>.*?):(?<addr>.*?):(?<ctx>.*?):(?<ctx2>.*?):(?<name>.*?):(?<hcode>.*?)\] (?<text>.*)");
        var match = regex.Match(outputData);
        var hp = new HookParam()
        {
            Handle = Convert.ToInt64(match.Groups[1].Value, 16),
            Pid = Convert.ToInt64(match.Groups[2].Value, 16),
            Address = Convert.ToInt64(match.Groups[3].Value, 16),
            Ctx = Convert.ToInt64(match.Groups[4].Value, 16),
            Ctx2 = Convert.ToInt64(match.Groups[5].Value, 16),
            Name = match.Groups[6].Value,
            HookCode = match.Groups[7].Value,
            Text = match.Groups[8].Value
        };

        _dataSubj.OnNext(hp);

        if (hp.Handle == 0)
        {
            _consoleOutput.Add(hp.Text);
            return;
        }

        foreach (var hookSetting in Setting.HookSettings)
        {
            if (Setting.HookCode.Equals(hp.HookCode)
                && hookSetting.ThreadType == TextractorSetting.TextThread.Text
                && (hookSetting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && hookSetting.SubThreadContext == hp.Ctx2)
            {
                _selectedDataSubj.OnNext(hp);
            }
            // XXX: hp.Name `Search` `Read` is different
            else if (Setting.HookCode.StartsWith('R') && hp.Name.Equals("READ"))
            {
                this.Log().Debug(hp.Text);
                _selectedDataSubj.OnNext(hp);
            }
        }
    }

    private bool IsX86Process(Process process)
    {
        // 32 bit system must be x86 process
        if (!Environment.Is64BitOperatingSystem)
        {
            return true;
        }

        bool runningInWow64 = false;
        try
        {
            if (!Kernel32.IsWow64Process(process.Handle, out runningInWow64))
            {
                throw new Win32Exception();
            }
        }
        catch (InvalidOperationException ex)
        {
            _gameProcesses.Remove(process);
            LogHost.Default.Debug(ex.Message);
        }
        catch (Exception ex)
        {
            LogHost.Default.Fatal(ex.ToString());
        }

        return runningInWow64;
    }
}
