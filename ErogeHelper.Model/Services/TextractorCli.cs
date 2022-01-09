using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using ErogeHelper.Model.DataServices.Interface;
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

    public void SetSetting(TextractorSetting setting) => Setting = setting;
    public TextractorSetting Setting { get; private set; } = null!;

    private readonly List<string> _consoleOutput = new();
    public List<string> GetConsoleOutputInfo() => _consoleOutput;

    private Process _textractorCli = null!;

    private IGameDataService? _gameDataService;
    private List<Process> GameProcesses => _gameDataService!.GameProcesses;

    public bool Injected { get; private set; } = false;

    /// <inheritdoc />
    public void InjectProcesses(IGameDataService? gameDataService = null)
    {
        if (Injected)
        {
            return;
        }
        Injected = true;

        _gameDataService = gameDataService;
        bool isX86 = GameProcesses.Any(p => IsX86Process(p));

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

        foreach (Process p in GameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("attach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"attach to PID {p.Id}.");
        }

        if (Setting.IsUserHook)
        {
            InsertHook(Setting.HookCode);
        }
    }

    public void InsertHook(string hookcode)
    {
        foreach (Process p in GameProcesses)
        {
            _textractorCli.StandardInput.WriteLine($"{hookcode} -P{p.Id}");
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"Insert code {hookcode} to PID {p.Id}.");
        }
    }

    public void ReAttachProcesses()
    {
        foreach (Process p in GameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("detach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"detach to PID {p.Id}.");
        }
    }

    public void RemoveHook(long address) => throw new InvalidOperationException();
    public void RemoveUselessHooks() => throw new InvalidOperationException();
    public void SearchRCode(string text) => throw new InvalidOperationException();

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

        var sentence = RemoveRepeatChar(match.Groups[8].Value);
        var hp = new HookParam()
        {
            Handle = Convert.ToInt64(match.Groups[1].Value, 16),
            Pid = Convert.ToInt64(match.Groups[2].Value, 16),
            Address = Convert.ToInt64(match.Groups[3].Value, 16),
            Ctx = Convert.ToInt64(match.Groups[4].Value, 16),
            Ctx2 = Convert.ToInt64(match.Groups[5].Value, 16),
            Name = match.Groups[6].Value,
            HookCode = match.Groups[7].Value,
            Text = sentence
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

    private static bool IsX86Process(Process process)
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
            LogHost.Default.Debug(ex.Message);
        }
        catch (Exception ex)
        {
            LogHost.Default.Debug(ex.ToString());
        }

        return runningInWow64;
    }

    private static string RemoveRepeatChar(string sentence)
    {
        int[] repeatNumbers = new int[sentence.Length + 1];
        var repeatNumber = 1;
        var prevChar = '\0';
        foreach (var nextChar in sentence)
        {
            if (nextChar == prevChar)
            {
                repeatNumber += 1;
            }
            else
            {
                prevChar = nextChar;
                repeatNumbers[repeatNumber] += 1;
                repeatNumber = 1;
            }
        }
        var (_, index) = repeatNumbers.Select((n, i) => (n, i)).Max();
        if (index == 1)
            return sentence;

        var newSentence = string.Empty;
        for (int i = 0; i < sentence.Length;)
        {
            newSentence += sentence[i];
            for (int j = i; j <= sentence.Length; ++j)
            {
                if (j == sentence.Length || sentence[i] != sentence[j])
                {
                    i += (j - i) % repeatNumber == 0 ? repeatNumber : 1;
                    break;
                }
            }
        }
        return newSentence;
    }
}
