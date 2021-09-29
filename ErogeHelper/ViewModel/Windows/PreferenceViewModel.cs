using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IPreferenceScreen, IEnableLogger
    {
        private readonly IWindowDataService _windowDataService;

        public bool LoseFocus => _windowDataService.LoseFocus;

        public RoutingState Router { get; } = new();

        public PreferenceViewModel(IWindowDataService? windowDataService = null)
        {
            _windowDataService = windowDataService ?? DependencyInject.GetService<IWindowDataService>();
            //Router.NavigationChanged.Subscribe(x => this.Log().Debug(x));
        }

        [Reactive]
        public string PageHeader { get; set; } = "Untitled";
    }
}
