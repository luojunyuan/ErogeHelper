using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Migration;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using ErogeHelper.Model.Factory;
using ErogeHelper.Model.Factory.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ErogeHelper
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            // initialize Bootstrapper, include EventAggregator, AssemblySource, IOC, Application event
            Initialize();
        }

        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Command line parameters</param>
        protected override async void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            // FIXME: 引发的异常:“System.IO.FileNotFoundException”(位于 System.Private.CoreLib.dll 中)
            // Put the database update into a scope to ensure that all resources will be disposed.
            using var scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.UpdateEhDatabase();

            if (e.Args.Length == 0)
            {
                await DisplayRootViewFor<SelectProcessViewModel>().ConfigureAwait(false);
                return;
            }

            var gamePath = e.Args[0];
            var gameDir = gamePath.Substring(0, gamePath.LastIndexOf('\\'));
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Not a valid game path \"{gamePath}\"", gamePath);
            Log.Info($"Game's path: {gamePath}");
            Log.Info($"Locate Emulator status: {e.Args.Contains("/le") || e.Args.Contains("-le")}");

            var ehGlobalValueRepository = _serviceProvider.GetRequiredService<EhGlobalValueRepository>();

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

            IEnumerable<Process> tmp;
            (tmp, ehGlobalValueRepository.MainProcess) =
                Utils.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
            var gameProcesses = tmp.ToList();
            if (!gameProcesses.Any())
            {
                await MessageBox.ShowAsync($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper")
                    .ConfigureAwait(false);
                return;
            }

            var gameWindowHooker = _serviceProvider.GetRequiredService<IGameWindowHooker>();
            var ehDbRepository = _serviceProvider.GetRequiredService<EhDbRepository>();
            var ehServerApi = _serviceProvider.GetRequiredService<IEhServerApiService>();
            var textractorService = _serviceProvider.GetRequiredService<ITextractorService>();

            _ = gameWindowHooker.SetGameWindowHookAsync();

            ehGlobalValueRepository.GamePath = gamePath;
            var md5 = Utils.GetFileMd5(gamePath);
            ehGlobalValueRepository.Md5 = md5;

            var settingJson = string.Empty;
            var gameInfo = await ehDbRepository.GetGameInfoAsync(md5).ConfigureAwait(false);
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
                        var content = resp.Content ?? new GameSettingResponse();
                        settingJson = content.GameSettingJson;
                        await ehDbRepository.SetGameInfoAsync(new GameInfoTable
                        {
                            Md5 = md5,
                            GameIdList = content.GameId.ToString(),
                            TextractorSettingJson = content.GameSettingJson,
                        }).ConfigureAwait(false);
                    }
                    Log.Debug($"EHServer: {resp.StatusCode} {resp.Content}");
                }
                catch (HttpRequestException ex)
                {
                    Log.Warn("Can't connect to internet", ex);
                }
            }

            if (settingJson == string.Empty)
            {
                Log.Info("Not find game hook setting, open hook panel.");
                await Application.Dispatcher.InvokeAsync(
                    async () => await DisplayRootViewFor<HookConfigViewModel>().ConfigureAwait(false));
                textractorService.InjectProcesses(gameProcesses);
                return;
            }

            var windowManager = _serviceProvider.GetRequiredService<IWindowManager>();
            var eventAggregator = _serviceProvider.GetRequiredService<IEventAggregator>();
            var ehConfigRepository = _serviceProvider.GetRequiredService<EhConfigRepository>();

            var textractorSetting = JsonSerializer.Deserialize<TextractorSetting>(settingJson) ?? new TextractorSetting();
            textractorService.InjectProcesses(gameProcesses, textractorSetting);

            await windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            _ = ehConfigRepository.UseOutsideWindow
                ? eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"))
                : eventAggregator.PublishOnUIThreadAsync(
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
                .AddLogging(lb => lb.AddSerilog())
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