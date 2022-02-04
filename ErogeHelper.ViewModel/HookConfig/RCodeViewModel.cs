using System.Reactive;
using ReactiveUI;

namespace ErogeHelper.ViewModel.HookConfig;

public class RCodeViewModel : ReactiveObject
{
    public Interaction<Unit, string> Show { get; set; } = new();
}
