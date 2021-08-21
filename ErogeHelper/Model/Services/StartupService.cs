using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
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
        private readonly IEHConfigDataService _ehConfigDataService;
        private readonly IEhDbRepository _ehDbRepository;

        public StartupService(
            IGameDataService? gameDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IEHConfigDataService? ehConfigDataService = null,
            IEhDbRepository? ehDbRepository = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEHConfigDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
        }

        public void StartByInjectButton(string gamePath)
        {
            throw new NotImplementedException();
        }

        public void StartFromCommandLine(string gamePath, bool leEnable)
        {
            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\".", gamePath);
            }

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {leEnable}");

            var gameDir = Path.GetDirectoryName(gamePath)!;

            // TODO: Refactor
            var gameInfo = _ehDbRepository.GetGameInfo(Utils.Md5Calculate(File.ReadAllBytes(gamePath)));
            if (gameInfo is not null && gameInfo.UseCloudSave)
            {
                // 能进来说明需要的信息都具备了
                this.Log().Debug("Game use cloud savedata");
                // 几种有限的情境，不会产生其他情形
                // 如果本地的档与云的一致，什么也不做。（上次退出游戏时已经更新到最新了）（标识肯是相同计算机不考虑其他情况）
                // 如果云的档比本地的旧，并且标识是相同计算机（说明上次退出游戏时没能成功更新到云），该。。。立即上传同步或者不管
                // 如果云的档比本地的旧，并且标识是不同计算机（说明断网情况下在本地游玩），需要弹窗提示用户手动替换。
                // 如果云的档比本地的新，并且标识是不同计算机，下载同步
                // 以上 断网 和“没有使用eh”意义是相同的
                // Exceptions
                // 云的档比本地的新，并且标识相同计算机（说明用户手动删除了存档），弹窗提示用户。
                // 断网情况，onedrive或nas有不同情境
                // 考虑游戏路径改变的情况，需要检查存档目录的存在状况
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

            try
            {
                _gameDataService.LoadData(gamePath);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Language.Strings.MessageBox_TimeoutInfo, Language.Strings.Common_AppName);
                App.Terminate();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            if (File.Exists(Path.Combine(gameDir, "UnityPlayer.dll")))
            {
                throw new NotImplementedException("Not Support Unity game yet");
            }

            if (gameInfo is null)
            {
                _ehDbRepository.SetGameInfo(new() { Md5 = _gameDataService.Md5 });
            }

            //
            DependencyInject.ShowView<MainGameViewModel>();
        }
    }
}
