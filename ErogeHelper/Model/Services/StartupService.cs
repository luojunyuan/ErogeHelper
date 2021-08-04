using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Windows;
using ModernWpf;
using Splat;

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

        public void StartByInjectButton()
        {
            throw new NotImplementedException();
        }

        public void StartFromCommandLine(string[] args)
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
                    Thread.Sleep(ConstantValues.WaitNWJSGameStartDelay);
                }
            }

            try 
            {
                _gameDataService.LoadData(gamePath);
            }
            catch (ArgumentNullException ex) when (ex.ParamName is not null && ex.ParamName.Equals("mainProcess"))
            {
                MessageBox.Show(Language.Strings.MessageBox_TimeoutInfo, Language.Strings.Common_AppName);
                App.Terminate();
                return;
            }

            _gameWindowHooker.SetGameWindowHook(_gameDataService.MainProcess);

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
