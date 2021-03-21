using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
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
            var windowManager = _container.GetInstance<IWindowManager>();

            if (e.Args.Length == 0)
            {
                await DisplayRootViewFor<SelectProcessViewModel>().ConfigureAwait(false);
                return;
            }

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

            // Basic tools
            _container.PerRequest<IWindowManager, WindowManager>();

            // ViewModels
            _container.Singleton<GameViewModel>();
            _container.PerRequest<SelectProcessViewModel>();

            // Services
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _container.Instance(new EhConfigRepository(appDataDir));
            _container.Singleton<ITextractorService, TextractorService>();
            _container.PerRequest<IGameViewModelDataService, GameViewModelDataService>();
            _container.PerRequest<ISelectProcessDataService, SelectProcessDataService>();
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