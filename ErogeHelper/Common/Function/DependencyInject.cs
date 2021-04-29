using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;
using ReactiveUI;
using Splat;

namespace ErogeHelper.Common.Function
{
    public static class DependencyInject
    {
        public static void Register()
        {
            // Locator.CurrentMutable.InitializeSplat();
            // Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);
            Locator.CurrentMutable.Register(() => new MainGameWindow(), typeof(IViewFor<MainGameViewModel>));
            
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
            
            // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning
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