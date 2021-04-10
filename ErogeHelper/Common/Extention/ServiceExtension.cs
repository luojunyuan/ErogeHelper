using Caliburn.Micro;
using ErogeHelper.Model.Factory;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Migration;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Linq;

namespace ErogeHelper.Common.Extention
{
    public static class ServiceExtension
    {
        /// <summary>
        /// Update the database
        /// </summary>
        public static void UpdateEhDatabase(this IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp(); // System.IO.FileNotFoundException
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services, Type types)
        {
            types.Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => services.TryAddTransient(
                    viewModelType, viewModelType));

            services.ReAddSingleton<GameViewModel>();

            return services;
        }

        public static IServiceCollection AddCaliburnMicroTools(this IServiceCollection services)
        {
            services.TryAddSingleton<IEventAggregator, EventAggregator>();
            services.TryAddSingleton<IWindowManager, WindowManager>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFile = Path.Combine(appDataDir, "eh.db");
            var connectString = $"Data Source={dbFile}";

            services.TryAddSingleton<EhGlobalValueRepository>();

            services.TryAddScoped(_ => new EhConfigRepository(appDataDir));
            services.TryAddScoped(_ => new EhDbRepository(connectString));

            // XXX: FluentMigrator has too many dependencies... https://github.com/fluentmigrator/fluentmigrator/issues/982
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectString)
                    .ScanIn(typeof(AddGameInfoTable).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            return services;
        }

        public static IServiceCollection AddEhServer(this IServiceCollection services)
        {
            services.TryAddScoped<IEhServerApiService>(provider =>
            {
                var url = provider.GetRequiredService<EhConfigRepository>().EhServerBaseUrl;
                return new EhServerApiService(url);
            });

            return services;
        }

        public static IServiceCollection AddOtherModules(this IServiceCollection services)
        {
            // Services
            services.TryAddSingleton<ITextractorService, TextractorService>();
            services.TryAddSingleton<IGameWindowHooker, GameWindowHooker>();

            services.TryAddTransient<IGameDataService, GameDataService>();
            services.TryAddTransient<ISelectProcessDataService, SelectProcessDataService>();
            services.TryAddTransient<IHookDataService, HookDataService>();
            services.TryAddTransient<IDictFactory, DictFactory>();
            services.TryAddTransient<IMeCabService, MeCabService>();
            services.TryAddTransient<ITouchConversionHooker, TouchConversionHooker>();

            return services;
        }

        private static void ReAddSingleton<T>(this IServiceCollection services) =>
            services.Remove<T>().TryAddSingleton(typeof(T));

        private static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            var serviceDescriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T));

            foreach (var serviceDescriptor in serviceDescriptors.ToList())
            {
                services.Remove(serviceDescriptor);
            }

            return services;
        }
    }
}