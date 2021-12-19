using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Dialogs
{
    public class HCodeViewModel : ReactiveObject
    {
        public Interaction<Unit, string> Show { get; set; } = new();

        [Reactive]
        public string HookCode { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> SearchCode { get; } = ReactiveCommand.Create(() => { });
    }
}
