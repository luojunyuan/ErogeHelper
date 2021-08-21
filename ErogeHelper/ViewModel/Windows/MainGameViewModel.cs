using ErogeHelper.Common;
using ReactiveUI;
using System.Reactive;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject
    {
        public MainGameViewModel()
        {
            Interact = ReactiveCommand.Create(() =>
            {
                DependencyInject.ShowView<SavedataSyncViewModel>();
            });
        }

        public ReactiveCommand<Unit, Unit> Interact { get; }
    }
}
