using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using var scope = _serviceProvider.CreateScope();
            Utils.UpdateEhDatabase(scope.ServiceProvider);

            if (e.Args.Length == 0)
            {
                await DisplayRootViewFor<SelectProcessViewModel>().ConfigureAwait(false);
                return;
            }

            var windowManager = _serviceProvider.GetService<IWindowManager>();
            var ehConfigRepository = _serviceProvider.GetService<EhConfigRepository>();

            Log.Debug("a");
            await windowManager.ShowWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            Log.Debug("b");
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
            services.AddSingleton<GameViewModel>();
            services.AddTransient<SelectProcessViewModel>();
            services.AddTransient<HookConfigViewModel>();

            services.AddSingleton<HookViewModel>();

            // Services, no helper, manager
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            services.AddSingleton(configRepo);
            services.AddSingleton<IEhServerApi>(new EhServerApi(configRepo));
            services.AddSingleton<ITextractorService, TextractorService>();
            services.AddSingleton<IGameWindowHooker, GameWindowHooker>();
            services.AddTransient<IGameViewModelDataService, GameViewModelDataService>();
            services.AddTransient<ISelectProcessDataService, SelectProcessDataService>();

            // Database migration
            var dbFile = Path.Combine(configRepo.AppDataDir, "eh.db");
            // XXX: too many dependencies... https://github.com/fluentmigrator/fluentmigrator/issues/982
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQLite support to FluentMigrator
                    .AddSQLite()
                    // Set the connection string
                    // NOTE: Please ensure db file exist before using
                    .WithGlobalConnectionString($"Data Source={dbFile}")
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(AddGameInfoTable).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
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