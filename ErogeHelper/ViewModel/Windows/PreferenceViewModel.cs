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
        public RoutingState Router { get; } = new();

        public PreferenceViewModel()
        {
            //Router.NavigationChanged.Subscribe(x => this.Log().Debug(x));
        }

        [Reactive]
        public string PageHeader { get; set; } = "Untitled";
    }
}
