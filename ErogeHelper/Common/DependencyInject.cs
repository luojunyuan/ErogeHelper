using Config.Net;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Functions;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Controllers;
using ErogeHelper.View.Pages;
using ErogeHelper.View.Windows;
using ErogeHelper.ViewModel.Controllers;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Routing;
using ErogeHelper.ViewModel.Windows;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using System;
using System.Windows;
using Vanara.PInvoke.NetListMgr;

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
            Locator.CurrentMutable.Register(() => new PreferenceWindow(), typeof(IViewFor<PreferenceViewModel>));
            Locator.CurrentMutable.Register(() => new CloudSavedataPage(), typeof(IViewFor<CloudSavedataViewModel>));
            Locator.CurrentMutable.Register(() => new AboutPage(), typeof(IViewFor<AboutViewModel>));
            Locator.CurrentMutable.Register(() => new AssistiveTouch(), typeof(IViewFor<AssistiveTouchViewModel>));
            // ViewModel
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            Locator.CurrentMutable.Register(() => GetService<PreferenceViewModel>(), typeof(IPreferenceScreen));
            Locator.CurrentMutable.Register(() => new CloudSavedataViewModel());
            Locator.CurrentMutable.Register(() => new PreferenceViewModel());
            Locator.CurrentMutable.Register(() => new AboutViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
            // DataService
            Locator.CurrentMutable.RegisterLazySingleton(() => new GameDataService(), typeof(IGameDataService));
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ConfigurationBuilder<IEhConfigRepository>().UseJsonFile(EhContext.EhConfigFilePath).Build(),
                typeof(IEhConfigRepository));
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new EhDbRepository(EhContext.DbConnectString), typeof(IEhDbRepository));
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainWindowDataService(), typeof(IMainWindowDataService));
            // Service
            Locator.CurrentMutable.Register(() => new StartupService(), typeof(IStartupService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new GameWindowHooker(), typeof(IGameWindowHooker));
            Locator.CurrentMutable.RegisterLazySingleton(() => new SavedataSyncService(), typeof(ISavedataSyncService));

            Locator.CurrentMutable.RegisterLazySingleton(() => new NetworkListManager(), typeof(INetworkListManager));

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
    }
}