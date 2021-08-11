using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
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

        public StartupService(IGameDataService? gameDataService = null, IGameWindowHooker? gameWindowHooker = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
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

            // TODO
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

            // 想想窗口太小该怎么办
            // 有些handle没问题，等待就行了。筛选大小发送 √ 或筛选 大小赋值
            // handle有问题 不停刷新 直到发送？


            if (File.Exists(Path.Combine(gameDir, "UnityPlayer.dll")))
            {
                throw new NotImplementedException();
            }
            else
            { }

            //
            DependencyInject.ShowView<MainGameViewModel>();
        }
    }
}
