using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;

namespace ErogeHelper
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            // initialize Bootstrapper, include EventAggregator, AssemblySource, IOC, Application event
            Initialize();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            var windowManager = IoC.Get<IWindowManager>();

            Log.Debug("a");
            await windowManager.ShowWindowAsync(IoC.Get<GameViewModel>(), "InsideView");
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

            // Basic tools
            _container.PerRequest<IWindowManager, WindowManager>();

            // ViewModels
            _container.Singleton<GameViewModel>();

            // Services
            _container.Singleton<ITextractorService, TextractorService>();
        }

        #region Simple IoC Init
        private readonly SimpleContainer _container = new();

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
        #endregion
    }
}