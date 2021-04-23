using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ErogeHelper
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Command line parameters</param>
        protected override async void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // Resolve config repo first ensure `Roaming/ErogeHelper/` be created
            var ehConfigRepository = serviceProvider.GetRequiredService<EhConfigRepository>();

            // Put the database update into a scope to ensure that all resources will be disposed.
            serviceProvider.UpdateEhDatabase();

            if (e.Args.Length == 0)
            {
                await DisplayRootViewFor<SelectProcessViewModel>().ConfigureAwait(false);
                return;
            }

            // Eroge Helper run from command (or context menu)
            var gamePath = e.Args[0];
            var gameDir = gamePath[..gamePath.LastIndexOf('\\')];
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\"", gamePath);

            var alreadyHasProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gamePath)).Any();

            Log.Info($"Game's path: {gamePath}");
            Log.Info($"Locate Emulator status: {e.Args.Contains("/le") || e.Args.Contains("-le")}");

            if (e.Args.Contains("/le") || e.Args.Contains("-le"))
            {
                // Use Locate Emulator (x86 game only)
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
                // Direct start
                Process.Start(new ProcessStartInfo
                {
                    FileName = gamePath,
                    UseShellExecute = false,
                    WorkingDirectory = gameDir
                });
            }

            var ehGlobalValueRepository = serviceProvider.GetRequiredService<GameRuntimeDataRepo>();
            var ehDbRepository = serviceProvider.GetRequiredService<EhDbRepository>();

            // For nw.js based game
            if (File.Exists(Path.Combine(gameDir, "nw.pak")) && !alreadyHasProcess)
            {
                await Task.Delay(7000);
            }
            IEnumerable<Process> gameProcesses = Utils.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
            var (md5, gameProcess) = ehGlobalValueRepository.Init(gameProcesses);
            ehDbRepository.Md5 = md5;
            if (!gameProcesses.Any())
            {
                await MessageBox.ShowAsync($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper")
                    .ConfigureAwait(false);
                return;
            }

            var gameWindowHooker = serviceProvider.GetRequiredService<IGameWindowHooker>();
            var ehServerApi = serviceProvider.GetRequiredService<IEhServerApiService>();
            var textractorService = serviceProvider.GetRequiredService<ITextractorService>();

            _ = gameWindowHooker.SetGameWindowHookAsync(gameProcess, gameProcesses.ToList());

            var settingJson = string.Empty;
            var gameInfo = await ehDbRepository.GetGameInfoAsync().ConfigureAwait(false);
            if (gameInfo is not null)
            {
                settingJson = gameInfo.TextractorSettingJson;
            }
            else
            {
                try
                {
                    using var resp = await ehServerApi.GetGameSetting(md5).ConfigureAwait(false);
                    // For throw ApiException
                    //resp = await resp.EnsureSuccessStatusCodeAsync();
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var content = resp.Content ??
                                      throw new InvalidOperationException("Server response no content");
                        settingJson = content.GameSettingJson;
                        await ehDbRepository.SetGameInfoAsync(new GameInfoTable
                        {
                            Md5 = md5,
                            GameIdList = content.GameId.ToString(),
                            RegExp = content.RegExp,
                            TextractorSettingJson = content.GameSettingJson,
                        }).ConfigureAwait(false);
                    }

                    Log.Debug($"EHServer: {resp.StatusCode} {resp.Content}");
                }
                catch (HttpRequestException ex)
                {
                    Log.Warn("Can't connect to internet", ex);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Error(ex);
                }
            }

            if (settingJson == string.Empty)
            {
                Log.Info("Not find game hook setting, open hook panel.");
                textractorService.InjectProcesses(gameProcesses);
                await Application.Dispatcher.InvokeAsync(
                    async () => await DisplayRootViewFor<HookConfigViewModel>().ConfigureAwait(false));
                return;
            }

            var windowManager = serviceProvider.GetRequiredService<IWindowManager>();
            var eventAggregator = serviceProvider.GetRequiredService<IEventAggregator>();

            var textractorSetting = JsonSerializer.Deserialize<TextractorSetting>(settingJson) ?? new TextractorSetting();
            textractorService.InjectProcesses(gameProcesses, textractorSetting);

            await windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            if (ehConfigRepository.UseOutsideWindow)
            {
                _ = eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"));
            }

            _ = eventAggregator.PublishOnUIThreadAsync(
                new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));
        }

        protected override void Configure()
        {
            // Set Caliburn.Micro Conventions naming rule
            var config = new TypeMappingConfiguration
            {
                IncludeViewSuffixInViewModelNames = false,
                DefaultSubNamespaceForViewModels = "ViewModel",
                DefaultSubNamespaceForViews = "View",
                ViewSuffixList = new List<string> { "View", "Page", "Control", "Popup" }
            };
            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);

            // Set DI ConfigureServices
            _serviceProvider = new ServiceCollection()
                .AddViewModels(GetType())
                .AddCaliburnMicroTools()
                .AddRepositories()
                .AddEhServer()
                .AddOtherModules()
                //.AddLogging(lb => lb.AddSerilog()) // Not use
                .BuildServiceProvider();
        }

        #region Microsoft DependencyInjection Init

        private IServiceProvider? _serviceProvider;

        protected override object GetInstance(Type serviceType, string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var service = _serviceProvider?.GetService(serviceType);
                if (service is not null)
                    return service;
            }

            var requiredName = string.IsNullOrEmpty(key) ? serviceType.Name : key;
            throw new ArgumentException($"Could not locate any instances of contract {requiredName}.");
        }

        // QUESTION: May not work when call IoC.GetAllInstances()
        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return _serviceProvider.GetServices(type);
        }

        protected override void BuildUp(object instance)
        {
            // There is no property injection for microsoft DI by default
        }

        #endregion
    }
}