using Caliburn.Micro;
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
            Initialize();
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            log.Info("Started Logging");
            if (e.Args.Length == 0)
            {
                DisplayRootViewFor<SelectProcessViewModel>();
            }
        }
    }
}
