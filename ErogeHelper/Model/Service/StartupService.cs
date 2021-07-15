using ErogeHelper.Common;
using ErogeHelper.Common.Contract;
using ErogeHelper.Model.DataService.Interface;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service
{
    public class StartupService : IStartupService, IEnableLogger
    {
        public void StartFromCommandLine(List<string> args)
        {
            var gamePath = args[0];
            var gameDir = gamePath[..gamePath.LastIndexOf('\\')];
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\"", gamePath);

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {args.Contains("/le") || args.Contains("-le")}");

            if (!Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gamePath)).Any())
            {
                if (args.Contains("/le") || args.Contains("-le"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"\libs\x86\LEProc.exe",
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

            _gameDataService.LoadData(gamePath);
            if (!_gameDataService.GameProcesses.Any())
            {
                ModernWpf.MessageBox
                    .Show($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper");
                App.AppExit();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            DependencyInject.ShowView<MainGameViewModel>();
        }

        public StartupService(IGameDataService? gameDataService = null, IGameWindowHooker? gameWindowHooker = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
        }

        private readonly IGameDataService _gameDataService;
        private readonly IGameWindowHooker _gameWindowHooker;
    }
}
