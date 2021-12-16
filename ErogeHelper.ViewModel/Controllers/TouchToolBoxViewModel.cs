using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Controllers;

public class TouchToolBoxViewModel : ReactiveObject
{
    public TouchToolBoxViewModel(
        IGameInfoRepository? gameInfoRepository = null,
        IEHConfigRepository? ehConfigRepository = null)
    {
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        TouchToolBoxVisible = ehConfigRepository.UseTouchToolBox && gameInfoRepository.GameInfo.IsLoseFocus;
    }

    [Reactive]
    public bool TouchToolBoxVisible { get; set; }
}

