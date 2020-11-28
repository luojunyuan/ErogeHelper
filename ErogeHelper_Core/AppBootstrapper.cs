using Caliburn.Micro;
using ErogeHelper_Core.Common.Service;
using ErogeHelper_Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core
{
    class AppBootstrapper : BootstrapperBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AppBootstrapper));

        public AppBootstrapper()
        {
            // initialize Bootstrapper, include EventAggregator, AssemblySource, IOC, Application event
            Initialize(); 
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            log.Info("Started Logging");

            if (e.Args.Length == 0)
            {
                DisplayRootViewFor<SelectProcessViewModel>();
            }
            else
            {
                // 经过一番操作
                // 拿到进程 (list and HwndProc)
            }
        }

        protected override void Configure()
        {
            base.Configure();

            container.Singleton<IWindowManager, WindowManager>();

            container.PerRequest<ISelectProcessService, SelectProcessService>();
            container.PerRequest<SelectProcessViewModel>();
            container.PerRequest<HookConfigViewModel>();
        }

        #region SimpleContainer Init
        private readonly SimpleContainer container = new SimpleContainer();

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
        #endregion
    }
}
