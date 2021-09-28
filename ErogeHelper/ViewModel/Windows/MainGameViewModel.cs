using ErogeHelper.Common;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
        public MainGameViewModel(
            AssistiveTouchViewModel? assistiveTouchViewModel = null,
            IEhConfigRepository? ehConfigRepository = null)
        {
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyInject.GetService<AssistiveTouchViewModel>();
            ehConfigRepository ??= DependencyInject.GetService<IEhConfigRepository>();

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;
        }

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }
    }
}
