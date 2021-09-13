using ErogeHelper.Common;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using Splat;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
        public MainGameViewModel(AssistiveTouchViewModel? assistiveTouchViewModel = null)
        {
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyInject.GetService<AssistiveTouchViewModel>();
        }

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }
    }
}
