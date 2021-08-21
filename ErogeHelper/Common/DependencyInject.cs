using Config.Net;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Functions;
using ErogeHelper.Model.DAL.Migration;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Windows;
using ErogeHelper.ViewModel.Windows;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace ErogeHelper.Common
{
    public static class DependencyInject
    {
        public static void Register()
        {
            // Locator.CurrentMutable.InitializeSplat();
            // Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);

            // View
            Locator.CurrentMutable.Register(() => new MainGameWindow(), typeof(IViewFor<MainGameViewModel>));
            Locator.CurrentMutable.Register(() => new SavedataSyncWindow(), typeof(IViewFor<SavedataSyncViewModel>));
            // ViewModel
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            Locator.CurrentMutable.Register(() => new SavedataSyncViewModel());
            // DataService
            Locator.CurrentMutable.RegisterLazySingleton(() => new GameDataService(), typeof(IGameDataService));
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ConfigurationBuilder<IEHConfigDataService>().UseJsonFile(EHContext.EHConfigFilePath).Build(),
                typeof(IEHConfigDataService));
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new EhDbRepository(EHContext.DBConnectString), typeof(IEhDbRepository));
            // Service
            Locator.CurrentMutable.Register(() => new StartupService(), typeof(IStartupService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new GameWindowHooker(), typeof(IGameWindowHooker));

            // MISC
            // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
            Locator.CurrentMutable.Register(() => new CustomPropertyResolver(), typeof(ICreatesObservableForProperty));
        }

        public static T GetService<T>() => Locator.Current.GetService<T>() ??
                                           throw new InvalidOperationException(
                                               $"No service for type {typeof(T)} has been registered.");

        public static void ShowView<T>() where T : ReactiveObject
        {
            var view = GetService<IViewFor<T>>();
            if (view is not Window window)
                throw new TypeAccessException("View not implement IViewFor");
            window.Show();
        }

        public static void UpdateEhDatabase()
        {
            var microsoftServiceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(EHContext.DBConnectString)
                    .ScanIn(typeof(AddGameInfoTable).Assembly)
                    .ScanIn(typeof(AddUserTermTable).Assembly)
                    .ScanIn(typeof(ColumnAddSavedataCloud).Assembly)
                    .For.Migrations())
                .BuildServiceProvider(false);

            using var scope = microsoftServiceProvider.CreateScope();
            var runner = microsoftServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}