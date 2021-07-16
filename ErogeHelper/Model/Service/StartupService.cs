using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErogeHelper.Common;
using ErogeHelper.Common.Contract;
using ErogeHelper.Model.DataService.Interface;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using Splat;

namespace ErogeHelper.Model.Service
{
    class StartupService : IStartupService, IEnableLogger
    {
        private readonly IGameDataService _gameDataService;
        private readonly IGameWindowHooker _gameWindowHooker;

        public StartupService(IGameDataService? gameDataService = null, IGameWindowHooker? gameWindowHooker = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
        }

        public async Task StartFromCommandLine(string[] args)
        {
            string gamePath = args[0];
            string gameDir = Path.GetDirectoryName(gamePath)!;

            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\".", gamePath);
            }

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {args.Contains("/le") || args.Contains("-le")}");

            if (!Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gamePath)).Any())
            {
                if (args.Any(arg => arg is "/le" or "-le"))
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
                    await Task.Delay(ConstantValues.WaitNWJSGameStartDelay).ConfigureAwait(false);
                }
            }

            await _gameDataService.LoadDataAsync(gamePath).ConfigureAwait(false);
            if (!_gameDataService.GameProcesses.Any())
            {
                await ModernWpf.MessageBox
                    .ShowAsync($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper")
                    .ConfigureAwait(false);
                App.Terminate();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            await DependencyInject.ShowViewAsync<MainGameViewModel>().ConfigureAwait(false);
        }
    }
}
