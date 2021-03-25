using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Interface;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Page;
using ErogeHelper.ViewModel.Window;
using Refit;

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
            if (e.Args.Length == 0)
            {
                await DisplayRootViewFor<SelectProcessViewModel>().ConfigureAwait(false);
                return;
            }

            var windowManager = _container.GetInstance<IWindowManager>();
            var ehConfigRepository = _container.GetInstance<EhConfigRepository>();

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
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            // ViewModels
            _container.Singleton<GameViewModel>();
            _container.PerRequest<SelectProcessViewModel>();
            _container.PerRequest<HookConfigViewModel>();

            _container.Singleton<HookViewModel>();

            // Services, no helper, manager
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // EhConfigRepository instance is not created by container
            // Will SimpleContainer help me release the service?
            // Should I dispose the EhConfigRepository which may run for the entire app lifecycle? 
            // NOTE: SimpleContainer 似乎没有注册 Transient Scoped 这类功能，注册 PerRequest 的也不给我释放
            var configRepo = new EhConfigRepository(appDataDir);
            _container.Instance(configRepo);
            _container.Instance<IEhServerApi>(new EhServerApi(configRepo));
            _container.Singleton<ITextractorService, TextractorService>();
            _container.Singleton<IGameWindowHooker, GameWindowHooker>();
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