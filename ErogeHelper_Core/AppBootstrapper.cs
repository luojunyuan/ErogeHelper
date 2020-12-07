using Autofac;
using Caliburn.Micro;
using ErogeHelper_Core.Common.Service;
using ErogeHelper_Core.ViewModels;
using ErogeHelper_Core.ViewModels.Pages;
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
            var builder = new ContainerBuilder();

            // Register Basic Tools
            builder.RegisterType<WindowManager>()
                .AsImplementedInterfaces()
                .SingleInstance();
            //builder.RegisterType<EventAggregator>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            // Register ViewModels
            builder.RegisterType<SelectProcessViewModel>();
            builder.RegisterType<HookConfigViewModel>();

            builder.RegisterType<HookSettingPageViewModel>()
                .SingleInstance();

            // Register Servieces
            builder.RegisterType<SelectProcessService>()
                .AsImplementedInterfaces();

            Container = builder.Build();
        }

        #region Autofac Init
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        private static IContainer Container { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                if (Container.IsRegistered(service))
                    return Container.Resolve(service);
            }
            else
            {
                if (Container.IsRegisteredWithKey(key, service))
                    return Container.ResolveKeyed(key, service);
            }

            var msgFormat = "Could not locate any instances of contract {0}.";
            var msg = string.Format(msgFormat, key ?? service.Name);
            throw new Exception(msg);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>)Container.Resolve(type);
        }

        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }
        #endregion
    }
}
