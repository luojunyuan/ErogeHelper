using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataModel.Entity.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Windows;
using ModernWpf;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

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

            RunGame(gamePath, leEnable, gameDir);

            try
            {
                gameDataService.SearchingProcesses(gamePath);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Language.Strings.MessageBox_TimeoutInfo, "Eroge Helper");
                App.Terminate();
                return;
            }

            gameWindowHooker.SetGameWindowHook(gameDataService.MainProcess);

            DependencyResolver.ShowView<MainGameViewModel>();
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

            gameDataService.Init(md5, gamePath);

            var gameInfo = gameInfoRepository.GameInfo;
            if (gameInfo is null)
            {
                gameInfoRepository.AddGameInfo(new GameInfoTable() { Md5 = md5, });
            }

            if (ehConfigRepository.DPIByApplication && !Utils.AlreadyHasDpiCompatibilitySetting(gamePath))
            {
                Utils.SetDPICompatibilityAsApplication(gamePath);
            }
        }

        private static void RunGame(string gamePath, bool leEnable, string gameDir)
        {
            var gameAlreadyStart = Utils.GetProcessesByfriendlyName(Path.GetFileNameWithoutExtension(gamePath)).Any();

            if (!gameAlreadyStart)
            {
                if (leEnable)
                {
                    Process.Start(new ProcessStartInfo
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
                    Thread.Sleep(ConstantValues.WaitNWJSGameStartDelay);
                }
            }
        }
    }
}
