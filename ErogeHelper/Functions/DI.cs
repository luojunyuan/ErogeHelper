using System;
using System.Linq;
using System.Windows;
using Config.Net;
using ErogeHelper.Common.Functions;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.View.Controllers;
using ErogeHelper.View.Pages;
using ErogeHelper.View.Windows;
using ErogeHelper.ViewModel.Controllers;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using Splat;

namespace ErogeHelper.Functions
{
    internal static class DI
    {
        public static void RegisterServices()
        {
            // View
            Locator.CurrentMutable.Register<IViewFor<MainGameViewModel>>(() => new MainGameWindow());
            Locator.CurrentMutable.Register<IViewFor<AssistiveTouchViewModel>>(() => new AssistiveTouch());
            Locator.CurrentMutable.Register<IViewFor<TouchToolBoxViewModel>>(() => new TouchToolBox());
            //Locator.CurrentMutable.Register<IViewFor<TextViewModel>>(() => new TextWindow());
            Locator.CurrentMutable.Register<IViewFor<PreferenceViewModel>>(() => new PreferenceWindow());
            Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new GeneralPage());
            Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutPage());

            // ViewModel
            // XXX: These viewmodel no need to dispose in WhenActivated cause they all singleton
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new TouchToolBoxViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new PreferenceViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new GeneralViewModel());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AboutViewModel());

            // DataService
            // UPSTREAM: Bug waits for updating https://github.com/aloneguid/config/pull/124
            if (!System.IO.File.Exists(EHContext.ConfigFilePath))
            {
                System.IO.Directory.CreateDirectory(EHContext.RoamingEHFolder);
                var file = System.IO.File.CreateText(EHContext.ConfigFilePath);
                file.WriteLine("{}");
                file.Close();
            }
            Locator.CurrentMutable.RegisterLazySingleton(
                () => new ConfigurationBuilder<IEHConfigRepository>().UseJsonFile(EHContext.ConfigFilePath).Build());
            Locator.CurrentMutable.RegisterLazySingleton<IGameInfoRepository>(
                () => new GameInfoRepository(EHContext.DbConnectString));
            Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService());
            Locator.CurrentMutable.RegisterLazySingleton<IWindowDataService>(() => new WindowDataService());

            // Service
            Locator.CurrentMutable.RegisterLazySingleton<IUpdateService>(() => new UpdateService());
            Locator.CurrentMutable.RegisterLazySingleton<IGameWindowHooker>(() => new GameWindowHooker());
            Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHooker());
            //Locator.CurrentMutable.RegisterLazySingleton(() => new NetworkListManager(), typeof(INetworkListManager));

            // MISC
            // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
            Locator.CurrentMutable.RegisterLazySingleton<ICreatesObservableForProperty>(() => new CustomPropertyResolver());
        }
        
        public static void ShowView<T>() where T : ReactiveObject
        {
            var viewName = typeof(T).ToString()[..^9] // erase ViewModel suffix
                .Replace("Model", string.Empty) + "Window";
            var windowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);

            System.Windows.Window? targetWindow = null;
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
                if (targetWindow.WindowState == WindowState.Minimized)
                {
                    targetWindow.WindowState = WindowState.Normal;
                }
            }
            else
            {
                var view = DependencyResolver.GetService<IViewFor<T>>();
                if (view is not Window window)
                    throw new TypeAccessException("View not implement IViewFor");
                window.Show();
                window.Activate();
            }
        }
    }
}
