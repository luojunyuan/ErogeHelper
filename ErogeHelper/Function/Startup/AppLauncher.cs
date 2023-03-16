using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Extensions;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Services;
using Microsoft.Win32;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using WK.Libraries.SharpClipboardNS;

namespace ErogeHelper.Function.Startup;

/// <summary>
/// Some progress for initializing business
/// </summary>
internal static class AppLauncher
{
    public static void StartOptionalFunctions() => Task.Run(() =>
    {
        var ehconfig = DependencyResolver.GetService<IEHConfigRepository>();
        var gameInfo = DependencyResolver.GetService<IGameInfoRepository>();
        var textractorService = DependencyResolver.GetService<ITextractorService>();
        // FIXME: UserHook repeat inject for one game
        textractorService.SetSetting(JsonSerializer.Deserialize<TextractorSetting>(gameInfo.TextractorSettingJson) ?? new());
        if (ehconfig.StartupInjectProcess && !State.IsUnityGame && !gameInfo.UseClipboard)
        {
            textractorService.InjectProcesses();
        }

        var sharpClipboard = DependencyResolver.GetService<SharpClipboard>();
        gameInfo.WhenAnyValue(x => x.UseClipboard)
            .Subscribe(v => sharpClipboard.MonitorClipboard = v);
        sharpClipboard.Events().ClipboardChanged
            .Where(e => e.SourceApplication.ID != Environment.ProcessId // SourceApplication.ID not equal PID
                && e.ContentType == SharpClipboard.ContentTypes.Text)
            .Select(e => e.Content.ToString() ?? string.Empty)
            .DistinctUntilChanged()
            .Subscribe(text => textractorService.AddClipboardText(text));

        if (ehconfig.ShowTextWindow == true)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => new View.TextDisplay.TextWindow().Show());
        }
    });
    public static IObservable<bool> FullScreenWatcher(IGameWindowHooker gameWindowHooker)
    {
        // Fullscreen Change Sources
        var gameResolutionChanged = gameWindowHooker.GamePosUpdated
            .Select(pos => (pos.Width, pos.Height))
            .DistinctUntilChanged()
            .ToUnit();
        var monitorResolutionChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
            // ThreadName .NET System Events
            x => SystemEvents.DisplaySettingsChanged += x,
            x => SystemEvents.DisplaySettingsChanged -= x)
            .ToUnit();
        var dpiChanged = State.DpiChanged
            .DistinctUntilChanged()
            .ToUnit();

        return monitorResolutionChanged
            .Merge(gameResolutionChanged)
            .Merge(dpiChanged)
            .Select(_ => WpfHelper.IsGameForegroundFullscreen(State.GameRealWindowHandle))
            .DistinctUntilChanged();
    }
    public static void RunGame(string gamePath, bool leEnable)
    {
        LogHost.Default.Info($"Locate Emulator status: {leEnable}");
        var gameAlreadyStart = Utils.GetProcessesByFriendlyName(Path.GetFileNameWithoutExtension(gamePath)).Any();

        if (gameAlreadyStart)
            return;

        if (DependencyResolver.GetService<IEHConfigRepository>().DPICompatibilityByApplication &&
            !RegistryModifier.IsDpiCompatibilitySet(gamePath))
        {
            RegistryModifier.SetDPICompatibilityAsApplication(gamePath);
        }

        if (leEnable)
        {
            // NOTE: LE may throw AccessViolationException which can not be caught
            var leProc = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "libs", "x86", "LEProc.exe"),
                UseShellExecute = false,
                Arguments = File.Exists(gamePath + ".le.config")
                    ? $"-run \"{gamePath}\""
                    : $"\"{gamePath}\""
            });
            leProc?.Kill();
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = false,
                WorkingDirectory = State.GameFolder
            });
        }

        // Wait for nw.js based game start multi-process
        if (File.Exists(Path.Combine(State.GameFolder, "nw.pak")))
        {
            var WaitNWjsGameStartDelay = TimeSpan.FromSeconds(7);
            Thread.Sleep(WaitNWjsGameStartDelay);
        }
    }

    public const int UIMinimumResponseTime = 50;

    /// <summary>
    /// Get all processes of the game (timeout 20s).
    /// </summary>
    /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
    public static (Process, List<Process>)? ProcessCollect(string friendlyName)
    {
        var spendTime = new Stopwatch();
        spendTime.Start();
        var procList = Utils.GetProcessesByFriendlyName(friendlyName);
        var mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        const int WaitGameStartTimeout = 20000;

        while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < WaitGameStartTimeout)
        {
            Thread.Sleep(UIMinimumResponseTime);
            procList = Utils.GetProcessesByFriendlyName(friendlyName);
            mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        }
        spendTime.Stop();

        if (mainProcess is null)
            return null;

        LogHost.Default.Info(
            $"{procList.Count} Process(es) and MainWindowHandle Found. {spendTime.ElapsedMilliseconds}ms");

        return (mainProcess, procList);
    }
}
