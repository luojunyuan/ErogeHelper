using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using ErogeHelper.Common.Entities;
using ErogeHelper.Function;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services;

public class TextractorCli : ITextractorService, IEnableLogger
{
    private readonly Subject<HookParam> _dataSubj = new();
    private readonly Subject<HookParam> _selectedDataSubj = new();
    private readonly List<string> _consoleOutput = new();

    public IObservable<HookParam> Data => _dataSubj;

    public IObservable<HookParam> SelectedData => _selectedDataSubj;

    public TextractorSetting Setting { get; private set; } = new();


    private Process _textractorCli = null!;
    private static ReadOnlyCollection<Process> GameProcesses => State.GameProcesses.AsReadOnly();

    public bool Injected { get; private set; }

    /// <inheritdoc />
    public void InjectProcesses()
    {
        if (Injected == true) return;
        Injected = true;

        var is32Bit = GameProcesses.Any(Is32BitProcess);

        var textractorCliPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "libs",
            is32Bit ? "x86" : "x64",
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
            Task.Run(async () => 
            {
                await Task.Delay(5000).ConfigureAwait(false);
                InsertHook(Setting.HookCode);
            });
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

    public async ValueTask ReAttachProcesses()
    {
        foreach (Process p in GameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("detach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
        }

        await Task.Delay(500).ConfigureAwait(false);

        _consoleOutput.Clear();
        foreach (Process p in GameProcesses)
        {
            _textractorCli.StandardInput.WriteLine("attach -P" + p.Id);
            _textractorCli.StandardInput.Flush();
            this.Log().Debug($"Retach to PID {p.Id}.");
        }
    }

    public bool FullSupport { get; } = false;
    public void RemoveHook(long address) => throw new InvalidOperationException();
    public void SearchRCode(string text) => throw new InvalidOperationException();

    public void SetSetting(TextractorSetting setting) => Setting = setting;
    public string GetConsoleOutputInfo() => string.Join('\n', _consoleOutput);

    public void AddClipboardText(string text)
    {
        if (text.Length > 5000)
            return;

        var hp = new HookParam()
        {
            Handle = 1,
            Pid = 0,
            Address = 0,
            Ctx = -1,
            Ctx2 = -1,
            Name = "Clipboard",
            HookCode = "HB0@0",
            Text = text
        };

        _dataSubj.OnNext(hp);

        if (Setting.HookCode.Equals(hp.HookCode, StringComparison.Ordinal))
        {
            this.Log().Debug(hp.Text);
            _selectedDataSubj.OnNext(hp);
        }
    }

    private void OutputDataRetrieveCallback(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not { } outputData ||
            !outputData.StartsWith('[') ||
            outputData.Length > 5000)
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
            if (Setting.HookCode.Equals(hp.HookCode, StringComparison.Ordinal)
                && hookSetting.ThreadType == TextractorSetting.TextThread.Text
                && (hookSetting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && hookSetting.SubThreadContext == hp.Ctx2)
            {
                this.Log().Debug(hp.Text);
                _selectedDataSubj.OnNext(hp);
            }
            // XXX: hp.Name `Search` `Read` is different
            else if (Setting.HookCode.StartsWith('R') && hp.Name.Equals("READ", StringComparison.Ordinal))
            {
                this.Log().Debug(hp.Text);
                _selectedDataSubj.OnNext(hp);
            }
        }
    }

    private static bool Is32BitProcess(Process process)
    {
        // 32 bit system must be x86_32 process
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

        var newSentence = new StringBuilder();
        for (int i = 0; i < sentence.Length;)
        {
            newSentence.Append(sentence[i]);
            for (int j = i; j <= sentence.Length; ++j)
            {
                if (j == sentence.Length || sentence[i] != sentence[j])
                {
                    i += (j - i) % repeatNumber == 0 ? repeatNumber : 1;
                    break;
                }
            }
        }
        return newSentence.ToString();
    }
}
