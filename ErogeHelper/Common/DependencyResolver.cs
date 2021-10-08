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
using ErogeHelper.ViewModel.Windows;
using Punchclock;
using ReactiveUI;
using Splat;
using System;
using System.Windows;
using Vanara.PInvoke.NetListMgr;

namespace ErogeHelper.Common
{
    public static class DependencyResolver
    {
        public static void Register()
        {
            // Locator.CurrentMutable.InitializeSplat();
            // Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);

            // View
            Locator.CurrentMutable.Register<IViewFor<MainGameViewModel>>(() => new MainGameWindow());
            Locator.CurrentMutable.Register<IViewFor<AssistiveTouchViewModel>>(() => new AssistiveTouch());
            Locator.CurrentMutable.Register<IViewFor<PreferenceViewModel>>(() => new PreferenceWindow());
            Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new GeneralPage());
            Locator.CurrentMutable.Register<IViewFor<CloudSavedataViewModel>>(() => new CloudSavedataPage());
            Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutPage());
            // ViewModel
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
            Locator.CurrentMutable.Register(() => new PreferenceViewModel());
            Locator.CurrentMutable.Register(() => new GeneralViewModel());
            Locator.CurrentMutable.Register(() => new CloudSavedataViewModel());
            Locator.CurrentMutable.Register(() => new AboutViewModel());
            // DataService
            Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService());
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ConfigurationBuilder<IEhConfigRepository>().UseJsonFile(EhContext.EhConfigFilePath).Build());
            Locator.CurrentMutable.RegisterLazySingleton<IGameInfoRepository>(
                () => new GameInfoRepository(EhContext.DbConnectString));
            Locator.CurrentMutable.RegisterLazySingleton<IMainWindowDataService>(() => new MainWindowDataService());
            // Service
            Locator.CurrentMutable.Register<IStartupService>(() => new StartupService());
            Locator.CurrentMutable.RegisterLazySingleton<IGameWindowHooker>(() => new GameWindowHooker());
            Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHooker());
            Locator.CurrentMutable.RegisterLazySingleton<ISavedataSyncService>(() => new SavedataSyncService());

            Locator.CurrentMutable.RegisterLazySingleton(() => new NetworkListManager(), typeof(INetworkListManager));

            Locator.CurrentMutable.RegisterLazySingleton(() => new OperationQueue(int.MaxValue));

            // MISC
            // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
            Locator.CurrentMutable.Register<ICreatesObservableForProperty>(() => new CustomPropertyResolver());
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