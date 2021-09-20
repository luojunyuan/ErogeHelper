using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.DTO.Entity.Tables;
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

namespace ErogeHelper.Model.Services
{
    public class StartupService : IStartupService, IEnableLogger
    {
        private readonly IGameDataService _gameDataService;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IEhConfigRepository _ehConfigDataService;
        private readonly IEhDbRepository _ehDbRepository;
        //private readonly ISavedataSyncService _savedataSyncService;

        public StartupService(
            IGameDataService? gameDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IEhConfigRepository? ehConfigDataService = null,
            IEhDbRepository? ehDbRepository = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
        }

        public void StartFromCommandLine(string gamePath, bool leEnable)
        {
            var gameDir = Path.GetDirectoryName(gamePath) ?? throw new InvalidOperationException();

            InitializeGameDatas(gamePath, leEnable, gameDir);

            RunGame(gamePath, leEnable, gameDir);

            try
            {
                _gameDataService.SearchingProcesses(gamePath);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Language.Strings.MessageBox_TimeoutInfo, "Eroge Helper");
                App.Terminate();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            DependencyInject.ShowView<MainGameViewModel>();
        }

        private void InitializeGameDatas(string gamePath, bool leEnable, string gameDir)
        {
            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\".", gamePath);
            }

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {leEnable}");

            string md5;
            try
            {
                md5 = Utils.Md5Calculate(File.ReadAllBytes(gamePath));
            }
            catch (IOException ex) // game with suffix ".log"
            {
                this.Log().Debug(ex.Message);
                md5 = Utils.Md5Calculate(File.ReadAllBytes(
                    Path.Combine(gameDir, Path.GetFileNameWithoutExtension(gamePath) + ".exe")));
            }

            _gameDataService.Init(md5, gamePath);

            var gameInfo = _ehDbRepository.GameInfo;
            if (gameInfo is null)
            {
                _ehDbRepository.AddGameInfo(new GameInfoTable() { Md5 = md5, });
            }

            if (_ehConfigDataService.DPIByApplication && !Utils.AlreadyHasDpiCompatibilitySetting(gamePath))
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
