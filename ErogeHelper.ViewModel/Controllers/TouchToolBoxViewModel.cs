using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Controllers;

public class TouchToolBoxViewModel : ReactiveObject
{
    [ObservableAsProperty]
    public bool TouchToolBoxVisible { get; }
}

