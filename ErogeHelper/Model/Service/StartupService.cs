using ErogeHelper.Common;
using ErogeHelper.Common.Contract;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.DataService.Interface;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Model.Service
{
    class StartupService : IStartupService, IEnableLogger
    {
        public async Task StartFromCommandLine(StartupEventArgs e)
        {
            var gamePath = e.Args[0];
            var gameDir = gamePath[..gamePath.LastIndexOf('\\')];
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\"", gamePath);

            this.Log().Debug($"Game's path: {gamePath}");
            this.Log().Debug($"Locate Emulator status: {e.Args.Contains("/le") || e.Args.Contains("-le")}");
            
            if (!Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gamePath)).Any())
            {
                if (e.Args.Contains("/le") || e.Args.Contains("-le"))
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
                    await Task.Delay(ConstantValues.WaitNWJSGameStartDelay).ConfigureAwait(false);
                }
            }

            await _gameDataService.LoadDataAsync(gamePath).ConfigureAwait(false);
            if (!_gameDataService.GameProcesses.Any())
            {
                await ModernWpf.MessageBox
                    .ShowAsync($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper")
                    .ConfigureAwait(false);
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

            await DependencyInject.ShowViewAsync<MainGameViewModel>().ConfigureAwait(false);
        }

        public StartupService(IGameDataService? gameDataService = null, IGameWindowHooker? gameWindowHooker = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _gameWindowHooker = gameWindowHooker ?? DependencyInject.GetService<IGameWindowHooker>();
        }

        private IGameDataService _gameDataService;
        private IGameWindowHooker _gameWindowHooker;
    }
}
