using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Interface;
using ErogeHelper.Model.Repository.Migration;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Page;
using ErogeHelper.ViewModel.Window;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository.Entity;

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
        protected override async void OnStartup(object sender, StartupEventArgs e)
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
            Log.Info($"Game's path: {e.Args[0]}");
            Log.Info($"Locate Emulator status: {e.Args.Contains("/le")}");
            var windowManager = _serviceProvider.GetService<IWindowManager>();
            var eventAggregator = _serviceProvider.GetService<IEventAggregator>();
            var ehConfigRepository = _serviceProvider.GetService<EhConfigRepository>();
            var gameWindowHooker = _serviceProvider.GetService<IGameWindowHooker>();
            var ehDbRepository = _serviceProvider.GetService<EhDbRepository>();
            var ehServerApi = _serviceProvider.GetService<IEhServerApi>();
            var textractorService = _serviceProvider.GetService<ITextractorService>();

            if (e.Args.Contains("/le") || e.Args.Contains("-le"))
            {
                // Use Locate Emulator (only x86 game)
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

            (ehConfigRepository.GameProcesses, ehConfigRepository.MainProcess) =
                Utils.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
            if (!ehConfigRepository.GameProcesses.Any())
            {
                MessageBox.Show($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper");
                return ;
            }

            _ = gameWindowHooker.SetGameWindowHookAsync();

            ehConfigRepository.GamePath = gamePath;
            var md5 = Utils.GetFileMd5(gamePath);

            var settingJson = string.Empty;
            var gameInfo = ehDbRepository.GetGameInfo(md5);
            if (gameInfo is not null)
            {
                settingJson = gameInfo.GameSettingJson;
            }
            else
            {
                try
                {
                    using var resp = await ehServerApi.GetGameSetting(md5).ConfigureAwait(true);
                    // For throw ApiException
                    //resp = await resp.EnsureSuccessStatusCodeAsync();
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var content = resp.Content ?? new GameSetting();
                        settingJson = content.GameSettingJson;
                    }
                    Log.Debug($"{resp.StatusCode} {resp.Content}");
                }
                catch (HttpRequestException ex)
                {
                    Log.Warn("Can't connect to internet", ex);
                }
            }

            if (settingJson == string.Empty)
            {
                await windowManager.ShowWindowFromIoCAsync<HookConfigViewModel>().ConfigureAwait(false);
                return;
            }

            var gameSetting = JsonSerializer.Deserialize<GameTextSetting>(settingJson) ?? new GameTextSetting();
            gameSetting.Md5 = md5;
            ehConfigRepository.TextractorSetting = gameSetting;
            textractorService.InjectProcesses();

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
                ViewSuffixList = new List<string> { "View", "Page", "Control" }
            };
            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Basic tools
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IWindowManager, WindowManager>();

            // ViewModels
            //GetType().Assembly.GetTypes()
            //    .Where(type => type.IsClass)
            //    .Where(type => type.Name.EndsWith("ViewModel"))
            //    .ToList()
            //    .ForEach(viewModelType => services.AddTransient(
            //        viewModelType, viewModelType));
            services.AddSingleton<GameViewModel>();
            services.AddTransient<SelectProcessViewModel>();
            services.AddTransient<HookConfigViewModel>();

            services.AddSingleton<HookViewModel>();

            // Services, no helper, manager
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var dbFile = Path.Combine(configRepo.AppDataDir, "eh.db");
            var connectString = $"Data Source={dbFile}";
            services.AddSingleton(configRepo);
            services.AddSingleton<IEhServerApi>(new EhServerApi(configRepo));
            services.AddSingleton<ITextractorService, TextractorService>();
            services.AddSingleton<IGameWindowHooker, GameWindowHooker>();
            services.AddTransient<IGameViewModelDataService, GameViewModelDataService>();
            services.AddTransient<ISelectProcessDataService, SelectProcessDataService>();
            services.AddSingleton(new EhDbRepository(connectString));
            // XXX: too many dependencies... https://github.com/fluentmigrator/fluentmigrator/issues/982
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
                $"Could not locate any instances of contract {(key == string.Empty ? serviceType.Name : key)}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return _serviceProvider.GetServices(type);
        }
        #endregion
    }
}