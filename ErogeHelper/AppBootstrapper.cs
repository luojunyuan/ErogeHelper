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
using System.Text.Json;
using ErogeHelper.Model.Factory;
using ErogeHelper.Model.Factory.Interface;

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
            // Put the database update into a scope to ensure that all resources will be disposed.
            using var scope = _serviceProvider.CreateScope();
            Utils.UpdateEhDatabase(scope.ServiceProvider); 

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

            var ehGlobalValueRepository = _serviceProvider.GetService<EhGlobalValueRepository>();

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

            var gameWindowHooker = _serviceProvider.GetService<IGameWindowHooker>();
            var ehDbRepository = _serviceProvider.GetService<EhDbRepository>();
            var ehServerApi = _serviceProvider.GetService<IEhServerApiService>();
            var textractorService = _serviceProvider.GetService<ITextractorService>();

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
                // XXX: Correspond ehServerApi.GetGameSetting(md5).ConfigureAwait(false)
                await Application.Dispatcher.InvokeAsync(
                    async () => await DisplayRootViewFor<HookConfigViewModel>().ConfigureAwait(false));
                textractorService.InjectProcesses(gameProcesses); // TODO
                return;
            }

            var windowManager = _serviceProvider.GetService<IWindowManager>();
            var eventAggregator = _serviceProvider.GetService<IEventAggregator>();
            var ehConfigRepository = _serviceProvider.GetService<EhConfigRepository>();

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

            // Set DI
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // UNDONE: Add developer option in UI Preference-MSIC, to check all registered instances
            // ViewModels
            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => services.AddTransient(
                    viewModelType, viewModelType));
            services.AddSingleton<GameViewModel>();

            // Basic tools
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IWindowManager, WindowManager>();

            // Services
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var ehConfigRepository = new EhConfigRepository(appDataDir);
            var dbFile = Path.Combine(ehConfigRepository.AppDataDir, "eh.db");
            var connectString = $"Data Source={dbFile}";

            services.AddSingleton<EhGlobalValueRepository>();
            services.AddSingleton<ITextractorService, TextractorService>();
            services.AddSingleton<IGameWindowHooker, GameWindowHooker>();

            services.AddScoped(_ => ehConfigRepository);
            services.AddScoped<IEhServerApiService>(_ => new EhServerApiServiceService(ehConfigRepository));
            services.AddScoped(_ => new EhDbRepository(connectString));

            services.AddTransient<IGameDataService, GameDataService>();
            services.AddTransient<ISelectProcessDataService, SelectProcessDataService>();
            services.AddTransient<IHookDataService, HookDataService>();
            services.AddTransient<IDictFactory, DictFactory>();
            services.AddTransient<IMeCabService, MeCabService>();
            services.AddTransient<ITouchConversionHooker, TouchConversionHooker>();

            // XXX: FluentMigrator has too many dependencies... https://github.com/fluentmigrator/fluentmigrator/issues/982
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectString)
                    .ScanIn(typeof(AddGameInfoTable).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());
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

            throw new Exception(
                $"Could not locate any instances of contract {(string.IsNullOrEmpty(key) ? serviceType.Name : key)}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return _serviceProvider.GetServices(type);
        }

        protected override void BuildUp(object instance)
        {
            // There is no Property Injection for Microsoft DI
        }

        #endregion
    }
}