using ErogeHelper.Common;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IPreferenceScreen, IEnableLogger
    {
        public RoutingState Router { get; } = new();

        public PreferenceViewModel(IEhDbRepository? ehDbRepository = null)
        {
            //Router.NavigationChanged.Subscribe(x => this.Log().Debug(x));
            ehDbRepository ??= DependencyInject.GetService<IEhDbRepository>();

            Loaded = ReactiveCommand.Create(() =>
            {
                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(PreferenceWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });
        }

        public HWND PreferenceWindowHandle { get; set; }

        [Reactive]
        public string PageHeader { get; set; } = "Untitled";

        public ReactiveCommand<Unit, Unit> Loaded { get; init; }
    }
}
