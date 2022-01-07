using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ErogeHelper.Functions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Languages;
using ErogeHelper.ViewModel.Windows;
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using MessageBox = ModernWpf.MessageBox;

namespace ErogeHelper;

public class AppLauncher
{
    public static void StartFromCommandLine(string gamePath, bool leEnable)
    {
        var gameDataService = DependencyResolver.GetService<IGameDataService>();
        var gameWindowHooker = DependencyResolver.GetService<IGameWindowHooker>();
        var textractorService = DependencyResolver.GetService<ITextractorService>();
        var ehConfigRepository = DependencyResolver.GetService<IEHConfigRepository>();

        var gameDir = Path.GetDirectoryName(gamePath) ?? throw new InvalidOperationException();

        InitializeGameDatas(gameDataService, textractorService, gameWindowHooker, ehConfigRepository,
            gamePath, leEnable, gameDir);

        var leproc = RunGame(gamePath, leEnable, gameDir);

        try
        {
            gameDataService.SearchingProcesses(gamePath);
            leproc?.Kill();
        }
        catch (TimeoutException)
        {
            MessageBox.Show(Strings.MessageBox_TimeoutInfo, "Eroge Helper");
            App.Terminate();
            return;
        }

        // Game hook and transparent MainGameWindow is the core of EH
        gameWindowHooker.SetupGameWindowHook(gameDataService.MainProcess, gameDataService, RxApp.MainThreadScheduler);
        DI.ShowView<PreferenceViewModel>();

        // Optional functions
        Observable.Start(() =>
        {
            if (ehConfigRepository.InjectProcessByDefalut)
            {
                // Question: GameProcesses Ambiant Context?
                textractorService.InjectProcesses(gameDataService.GameProcesses);
            }

            if (!ehConfigRepository.HideTextWindow)
            {
                // Show And Close
                //_ = DependencyResolver.GetService<TextViewModel>();
                //_ = new View.Windows.TextWindow(); // UI thread?
            }
        });
    }

    private static void InitializeGameDatas(
        IGameDataService gameDataService,
        ITextractorService textractorService,
        IGameWindowHooker gameWindowHooker,
        IEHConfigRepository ehConfigRepository,
        string gamePath, bool leEnable, string gameDir)
    {
        var gameInfoRepository = DependencyResolver.GetService<IGameInfoRepository>();

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


        // TODO: Add try block to catch the error from new db version back to old one. Tip user the db would be rebuild and the old one would be deleted.
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
            textractorService.Setting = JsonSerializer.Deserialize<TextractorSetting>
                (gameInfo.TextractorSettingJson) ?? new();
        }
        catch (JsonException ex)
        {
            // For compatible
            LogHost.Default.Debug(ex.Message);
            textractorService.Setting = new();
        }

        if (ehConfigRepository.DPIByApplication && !WpfHelper.IsDpiCompatibilitySet(gamePath))
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
        var dpiChanged = AmbiantContext.DpiChanged
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
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(operation =>
            {
                switch (operation)
                {
                    case ViewOperation.TerminateApp:
                        App.Terminate();
                        break;
                }
            });
    }

    /// <returns>Return LE if enabled</returns>
    private static Process? RunGame(string gamePath, bool leEnable, string gameDir)
    {
        Process? leproc = null;
        var gameAlreadyStart = Utils.GetProcessesByFriendlyName(Path.GetFileNameWithoutExtension(gamePath)).Any();

        if (!gameAlreadyStart)
        {
            if (leEnable)
            {
                leproc = Process.Start(new ProcessStartInfo
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
                Thread.Sleep(ConstantValue.WaitNWJSGameStartDelayTime);
            }
        }

        return leproc;
    }
}
