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
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Windows;
using Vanara.PInvoke.NetListMgr;

namespace ErogeHelper
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
            Locator.CurrentMutable.Register<IViewFor<TouchToolBoxViewModel>>(() => new TouchToolBox());
            Locator.CurrentMutable.Register<IViewFor<PreferenceViewModel>>(() => new PreferenceWindow());
            Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new GeneralPage());
            Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutPage());
            // ViewModel
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new TouchToolBoxViewModel());
            Locator.CurrentMutable.Register(() => new PreferenceViewModel());
            Locator.CurrentMutable.Register(() => new GeneralViewModel());
            Locator.CurrentMutable.Register(() => new AboutViewModel());
            // DataService
            Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService());
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ConfigurationBuilder<IEhConfigRepository>().UseJsonFile(EhContext.EhConfigFilePath).Build());
            Locator.CurrentMutable.RegisterLazySingleton<IGameInfoRepository>(
                () => new GameInfoRepository(EhContext.DbConnectString));
            Locator.CurrentMutable.RegisterLazySingleton<IMainWindowDataService>(() => new MainWindowDataService());
            // Service
            Locator.CurrentMutable.RegisterLazySingleton<IGameWindowHooker>(() => new GameWindowHooker());
            Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHooker());

            Locator.CurrentMutable.RegisterLazySingleton(() => new NetworkListManager(), typeof(INetworkListManager));

            // MISC
            // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
            Locator.CurrentMutable.Register<ICreatesObservableForProperty>(() => new CustomPropertyResolver());
        }

        public static T GetService<T>() => Locator.Current.GetService<T>() ??
                                           throw new InvalidOperationException(
                                               $"No service for type {typeof(T)} has been registered.");

        public static void ShowView<T>() where T : ReactiveObject
        {
            var viewName = typeof(T).ToString()[..^9].Replace("Model", string.Empty) + "Window";
            var windowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);

            Window? targetWindow = null;
            Application.Current.Windows
                .Cast<Window>().ToList()
                .ForEach(w =>
                {
                    if (w.GetType() == windowType)
                    {
                        targetWindow = w;
                    }
                });

            if (targetWindow is not null)
            {
                targetWindow.Activate();
            }
            else
            {
                var view = GetService<IViewFor<T>>();
                if (view is not Window window)
                    throw new TypeAccessException("View not implement IViewFor");
                window.Show();
            }
        }
    }
}