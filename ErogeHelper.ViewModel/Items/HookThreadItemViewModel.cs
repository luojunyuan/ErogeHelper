using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Items;

public class HookThreadItemViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    public HookThreadItemViewModel()
    {
        var hookerService = DependencyResolver.GetService<ITextractorService>();

        this.WhenActivated(d =>
        {
            hookerService.Data
                .Where(hp => hp.Handle == Handle)
                .Select(hp => hp.Text)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(text => TotalText += "\n\n" + text).DisposeWith(d);
        });
    }

    public long Handle { get; set; }

    [Reactive]
    public bool IsTextThread { get; set; }

    [Reactive]
    public bool IsCharacterThread { get; set; }

    [Reactive]
    public string TotalText { get; set; } = string.Empty;

    [Reactive]
    public string HookCode { get; set; } = string.Empty;

    public string EngineName { get; set; } = string.Empty;

    public long Context { get; set; }

    public long SubContext { get; set; }
}
