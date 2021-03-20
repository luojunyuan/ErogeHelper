using System.Windows;
using Caliburn.Micro;
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
            await IoC.Get<IWindowManager>().ShowWindowAsync(new GameViewModel(), "InsideView");
        }

        protected override void Configure()
        {
            // Set Caliburn.Micro Conventions naming rule
            var config = new TypeMappingConfiguration
            {
                IncludeViewSuffixInViewModelNames = false,
                DefaultSubNamespaceForViewModels = "ViewModel",
                DefaultSubNamespaceForViews = "View",
                ViewSuffixList = new () { "View", "Page", "Control" }
            };
            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);
        }
    }
}