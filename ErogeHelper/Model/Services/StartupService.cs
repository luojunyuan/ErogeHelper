using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DAL.Entity.Tables;
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

namespace ErogeHelper.Model.Services
{
    public class StartupService : IStartupService, IEnableLogger
    {
        private readonly IGameDataService _gameDataService;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IEhConfigDataService _ehConfigDataService;
        private readonly IEhDbRepository _ehDbRepository;
        private readonly ISavedataSyncService _savedataSyncService;

        public StartupService(
            IGameDataService? gameDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IEhConfigDataService? ehConfigDataService = null,
            IEhDbRepository? ehDbRepository = null,
            ISavedataSyncService? savedataSyncService = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _savedataSyncService = savedataSyncService ?? DependencyInject.GetService<ISavedataSyncService>();
        }

        public void StartFromCommandLine(string gamePath, bool leEnable)
        {
            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\".", gamePath);
            }

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {leEnable}");

            var gameDir = Path.GetDirectoryName(gamePath) ?? throw new InvalidOperationException();

            var md5 = Utils.Md5Calculate(File.ReadAllBytes(gamePath));
            _gameDataService.Init(md5, gamePath);

            var gameInfo = _ehDbRepository.GameInfo;
            if (gameInfo is null)
            {
                _ehDbRepository.AddGameInfo(new GameInfoTable() { Md5 = md5, });
            }

            if (gameInfo is not null && gameInfo.UseCloudSave)
            {
                _savedataSyncService.DownloadSync(gameInfo);
            }

            // TODO: 更全面的进程检测
            if (!Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gamePath)).Any())
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

            if (File.Exists(Path.Combine(gameDir, "UnityPlayer.dll")))
            {
                throw new NotImplementedException("Not Support Unity game yet");
            }

            try
            {
                _gameDataService.SearchingProcesses(gamePath);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Language.Strings.MessageBox_TimeoutInfo, Language.Strings.Common_AppName);
                App.Terminate();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            DependencyInject.ShowView<MainGameViewModel>();
        }
    }
}
