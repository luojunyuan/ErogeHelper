using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Share;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Controllers
{
    public class TouchToolBoxViewModel : ReactiveObject
    {
        public TouchToolBoxViewModel(
            IGameInfoRepository? gameInfoRepository = null,
            IEhConfigRepository? ehConfigRepository = null)
        {
            gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();

            TouchToolBoxVisible = ehConfigRepository.UseTouchToolBox && gameInfoRepository.GameInfo.IsLoseFocus;
        }

        [Reactive]
        public bool TouchToolBoxVisible { get; set; }
    }
}

