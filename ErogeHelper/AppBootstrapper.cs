using Autofac;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Control;
using ErogeHelper.ViewModel.Pages;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ErogeHelper
{
    class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            // initialize Bootstrapper, include EventAggregator, AssemblySource, IOC, Application event
            Initialize();
        }

        /// <summary>
        /// Entry Point
        /// </summary>
        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                // Display select processes window
                DisplayRootViewFor<SelectProcessViewModel>();
            }
            else
            {
                // Startup by shell menu
                var gamePath = e.Args[0];
                var gameDir = gamePath.Substring(0, gamePath.LastIndexOf('\\'));
                Log.Info($"Game's path: {e.Args[0]}");
                Log.Info($"Locate Emulator status: {e.Args.Contains("/le")}");

                if (e.Args.Contains("/le"))
                {
                    // Use Locate Emulator
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"\libs\x86\LEProc.exe",
                        UseShellExecute = false,
                        Arguments = File.Exists(gamePath + ".le.config")
                                               ? $"-run \"{gamePath}\""
                                               : $"\"{gamePath}\""
                    });
                    // XXX: LE may throw AccessViolationException which can not be catch
                }
                else
                {
                    // Direct start
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = gamePath,
                        UseShellExecute = false,
                        WorkingDirectory = gameDir
                    });
                }
                // 🧀
                var findResult = MatchProcess.Collect(Path.GetFileNameWithoutExtension(gamePath));
                if (findResult != true)
                {
                    MessageBox.Show($"{Language.Strings.MessageBox_TimeoutInfo}", "Eroge Helper");
                    return;
                }

                // Cheak if there is eh.config file
                if (File.Exists(gamePath + ".eh.config"))
                {
                    GameConfig.Load(gamePath + ".eh.config");

                    Log.Info($"Get HCode {GameConfig.HookCode} from file " +
                        $"{Path.GetFileNameWithoutExtension(gamePath)}.exe.eh.config");
                    // Display text window
                    DisplayRootViewForAsync(typeof(GameViewModel));
                }
                else
                {
                    Log.Info("Not find xml config file, open hook panel.");
                    DisplayRootViewFor<HookConfigViewModel>();
                }

                Textractor.Init();
                GameHooker.Init();
            }
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

            var builder = new ContainerBuilder();

            // Register Serilog logger
            //builder.RegisterLogger();

            // Register basic tools
            builder.RegisterType<WindowManager>()
                .AsImplementedInterfaces()
                .SingleInstance();
            //builder.RegisterType<EventAggregator>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            // Register viewModels
            builder.RegisterType<SelectProcessViewModel>();
            builder.RegisterType<HookConfigViewModel>()
                .SingleInstance();
            builder.RegisterType<GameViewModel>()
                .SingleInstance();
            builder.RegisterType<PreferenceViewModel>()
                .SingleInstance();

            // Control
            builder.RegisterType<TextViewModel>()
                .SingleInstance();
            builder.RegisterType<CardViewModel>()
                .SingleInstance();

            // Pages
            builder.RegisterType<GeneralViewModel>()
                .SingleInstance();
            builder.RegisterType<MecabViewModel>()
                .SingleInstance();
            builder.RegisterType<HookViewModel>()
                .SingleInstance();
            builder.RegisterType<AboutViewModel>()
                .SingleInstance();
            builder.RegisterType<LogViewModel>()
                .SingleInstance();

            // Register services
            builder.RegisterType<SelectProcessService>()
                .AsImplementedInterfaces();
            builder.RegisterType<GameViewDataService>()
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<HookSettingPageService>()
                .AsImplementedInterfaces();

            // Register others
            builder.RegisterType<MecabHelper>()
                .SingleInstance();
            builder.RegisterType<DeepLHelper>()
                .SingleInstance();

            Container = builder.Build();

            Log.Info("Started Logging");
            // Active preview for saving hole console output info
            Container.Resolve<HookViewModel>();
            // For saving log info
            Container.Resolve<LogViewModel>();
            // Active this for checking mecabHelper
            Container.Resolve<MecabViewModel>();
        }

        #region Autofac Init
        private static IContainer Container { get; set; } = null!; // Actually this is null by default

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
