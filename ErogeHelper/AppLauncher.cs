using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using ErogeHelper.Functions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.Share.Languages;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using Splat;
using MessageBox = ModernWpf.MessageBox;

namespace ErogeHelper
{
    public class AppLauncher
    {
        public static void StartFromCommandLine(string gamePath, bool leEnable)
        {
            var gameDataService = DependencyResolver.GetService<IGameDataService>();
            var gameWindowHooker = DependencyResolver.GetService<IGameWindowHooker>();
            var ehConfigRepository = DependencyResolver.GetService<IEhConfigRepository>();
            var gameInfoRepository = DependencyResolver.GetService<IGameInfoRepository>();

            var gameDir = Path.GetDirectoryName(gamePath) ?? throw new InvalidOperationException();

            InitializeGameDatas(gameDataService, gameInfoRepository, ehConfigRepository, gamePath, leEnable, gameDir);

            RegisterInteractions();

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

            gameWindowHooker.SetupGameWindowHook(
                gameDataService.MainProcess, gameDataService, RxApp.MainThreadScheduler);

            gameDataService.InitFullscreenChanged(
                gameWindowHooker.GamePosUpdated
                    .Select(pos => (pos.Width, pos.Height))
                    .DistinctUntilChanged()
                    .SelectMany(_ => Interactions.CheckGameFullscreen.Handle(gameDataService.GameRealWindowHandle))
                    .DistinctUntilChanged());

            DI.ShowView<MainGameViewModel>();
        }

        private static void InitializeGameDatas(
            IGameDataService gameDataService,
            IGameInfoRepository gameInfoRepository,
            IEhConfigRepository ehConfigRepository,
            string gamePath, bool leEnable, string gameDir)
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


            // TODO: Add try block to catch the error from new db version back to old one. Tip user the db would be rebuild and the old one would be deleted.
            var gameInfo = gameInfoRepository.TryGetGameInfo();
            if (gameInfo is null)
            {
                gameInfoRepository.AddGameInfo(new GameInfoTable() { Md5 = md5, });
            }

            if (ehConfigRepository.DPIByApplication && !WpfHelper.AlreadyHasDpiCompatibilitySetting(gamePath))
            {
                WpfHelper.SetDPICompatibilityAsApplication(gamePath);
            }
        }

        private static void RegisterInteractions()
        {
            Interactions.CheckGameFullscreen
                .RegisterHandler(context =>
                    context.SetOutput(WpfHelper.IsGameForegroundFullscreen(context.Input)));

            Interactions.TerminateApp
                .RegisterHandler(context =>
                {
                    App.Terminate();
                    context.SetOutput(Unit.Default);
                });

            Interactions.MessageBoxConfirm
                .RegisterHandler(context =>
                {
                    var result = MessageBox.Show(context.Input, "Eroge Helper");
                    var yesOrNo = result switch
                    {
                        MessageBoxResult.OK => true,
                        MessageBoxResult.Yes => true,
                        MessageBoxResult.No => false,
                        MessageBoxResult.Cancel => false,
                        MessageBoxResult.None => false,
                        null => false,
                        _ => throw new InvalidOperationException(),
                    };
                    context.SetOutput(yesOrNo);
                });
        }

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
}
