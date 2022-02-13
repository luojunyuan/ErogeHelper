using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using Config.Net;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Languages;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.MainGame;
using ErogeHelper.ViewModel.TextDisplay;
using Microsoft.Win32;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using WK.Libraries.SharpClipboardNS;
using MessageBox = ModernWpf.MessageBox;

namespace ErogeHelper;

public static class AppLauncher
{
    public static void StartFromCommandLine(string gamePath, bool leEnable)
    {
        var gameDir = Path.GetDirectoryName(gamePath);
        ArgumentNullException.ThrowIfNull(gameDir);

        var gameDataService = DependencyResolver.GetService<IGameDataService>();
        var gameWindowHooker = DependencyResolver.GetService<IGameWindowHooker>();
        var textractorService = DependencyResolver.GetService<ITextractorService>();
        var ehConfigRepository = DependencyResolver.GetService<IEHConfigRepository>();
        var gameInfoRepository = DependencyResolver.GetService<IGameInfoRepository>();

        InitializeGameData(
            gameDataService, textractorService, gameWindowHooker, ehConfigRepository, gameInfoRepository,
            gamePath, gameDir, leEnable);

        var leProc = RunGame(gamePath, gameDir, leEnable);

        try
        {
            gameDataService.InitGameProcesses(gamePath);
            leProc?.Kill();
        }
        catch (TimeoutException)
        {
            MessageBox.Show(Strings.MessageBox_TimeoutInfo, "Eroge Helper");
            App.Terminate();
            return;
        }

        // Game hook and transparent MainGameWindow is the core of EH
        gameWindowHooker.SetupGameWindowHook(gameDataService.MainProcess, gameDataService, RxApp.MainThreadScheduler);
        DI.ShowView<MainGameViewModel>();

        if (!ehConfigRepository.HideTextWindow)
        {
            DI.ShowView<TextViewModel>();
        }

        // Clipboard in UI thread
        var sharpClipboard = DependencyResolver.GetService<SharpClipboard>();

        // Optional functions
        Observable.Start(() =>
        {
            if (!gameInfoRepository.GameInfo.UseClipboard)
            {
                sharpClipboard.MonitorClipboard = false;
            }
            sharpClipboard.Events().ClipboardChanged
                .Where(e => e.SourceApplication.ID != Environment.ProcessId
                    && e.ContentType == SharpClipboard.ContentTypes.Text)
                // TODO: Unity game special id check
                .Skip(1)
                .Select(e => e.Content.ToString() ?? string.Empty)
                .DistinctUntilChanged()
                .Subscribe(text => textractorService.AddClipboardText(text));

            if (ehConfigRepository.InjectProcessByDefault && !File.Exists(Path.Combine(gameDir, "UnityPlayer.dll")))
            {
                textractorService.InjectProcesses(gameDataService);
            }
        });
    }

    private static void InitializeGameData(
        IGameDataService gameDataService,
        ITextractorService textractorService,
        IGameWindowHooker gameWindowHooker,
        IEHConfigRepository ehConfigRepository,
        IGameInfoRepository gameInfoRepository,
        string gamePath, string gameDir, bool leEnable)
    {
        if (!File.Exists(gamePath))
        {
            throw new FileNotFoundException($"Not a valid game path \"{gamePath}\".", gamePath);
        }

        LogHost.Default.Debug($"Game's path: {gamePath}");
        LogHost.Default.Debug($"Locate Emulator status: {leEnable}");

        string md5;
        try
        {
            md5 = Utils.Md5Calculate(File.ReadAllBytes(gamePath));
        }
        catch (IOException ex) // game with suffix ".log"
        {
            LogHost.Default.Debug(ex.Message);
            md5 = Utils.Md5Calculate(File.ReadAllBytes(
                Path.Combine(gameDir, Path.GetFileNameWithoutExtension(gamePath) + ".exe")));
        }

        gameDataService.InitGameMd5AndPath(md5, gamePath);

        // Creating or reading the GameInfo table
        gameInfoRepository.InitGameMd5(md5);
        // NOTE: There is a >5mb large object heap allocate
        var gameInfo = gameInfoRepository.TryGetGameInfo();
        if (gameInfo is null)
        {
            gameInfoRepository.AddGameInfo(new GameInfoTable()
            {
                Md5 = md5,
                TextractorSettingJson = "{}"
            });
            gameInfo = gameInfoRepository.GameInfo;
        }

        try
        {
            textractorService.SetSetting(JsonSerializer.Deserialize<TextractorSetting>
                (gameInfo.TextractorSettingJson) ?? new());
        }
        catch (JsonException ex)
        {
            // For compatible
            LogHost.Default.Debug(ex.Message);
            textractorService.SetSetting(new());
        }

        if (ehConfigRepository.DPICompatibilityByApplication && !WpfHelper.IsDpiCompatibilitySet(gamePath))
        {
            WpfHelper.SetDPICompatibilityAsApplication(gamePath);
        }

#region Fullscreen Change Sources
        var gameResolutionChanged = gameWindowHooker.GamePosUpdated
            .Select(pos => (pos.Width, pos.Height))
            .DistinctUntilChanged()
            .Select(_ => Unit.Default);
        var monitorResolutionChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
            x => SystemEvents.DisplaySettingsChanged += x,
            x => SystemEvents.DisplaySettingsChanged -= x)
            .Select(_ => Unit.Default);
        var dpiChanged = State.DpiChanged
            .DistinctUntilChanged()
            .Select(_ => Unit.Default);
#endregion

        gameDataService.InitFullscreenChanged(
            monitorResolutionChanged
                .Merge(gameResolutionChanged)
                .Merge(dpiChanged)
                .Select(_ => WpfHelper.IsGameForegroundFullscreen(gameDataService.GameRealWindowHandle))
                .DistinctUntilChanged().Publish());

        gameWindowHooker.WhenViewOperated
            .Where(op => op == ViewOperation.TerminateApp)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => App.Terminate());
    }

    private const int WaitNWjsGameStartDelayTime = 7000;

    /// <returns>Return LE if enabled</returns>
    private static Process? RunGame(string gamePath, string gameDir, bool leEnable)
    {
        var gameAlreadyStart = Utils.GetProcessesByFriendlyName(Path.GetFileNameWithoutExtension(gamePath)).Any();

        if (gameAlreadyStart) 
            return null;
        
        Process? leProc = null;
        if (leEnable)
        {
            leProc = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "libs", "x86", "LEProc.exe"),
                UseShellExecute = false,
                Arguments = File.Exists(gamePath + ".le.config")
                    ? $"-run \"{gamePath}\""
                    : $"\"{gamePath}\""
            });
            // NOTE: LE may throw AccessViolationException which can not be catch
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = false,
                WorkingDirectory = gameDir
            });
        }

        // Wait for nw.js based game start multi-process
        if (File.Exists(Path.Combine(gameDir, "nw.pak")))
        {
            Thread.Sleep(WaitNWjsGameStartDelayTime);
        }

        return leProc;
    }
}
