using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Function;
using ErogeHelper.Model.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.HookConfig;

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
                .Do(hp => Context = hp.Ctx) // Context may be inaccurate
                .Select(hp => hp.Text)
                .Where(text => text.Length < MaxLength)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(text => TotalText += "\n\n" + text).DisposeWith(d);
        });
    }

    private const int MaxLength = 1000;

    public long Handle { get; init; }

    [Reactive]
    public int Index { get; init; }

    [Reactive]
    public bool IsTextThread { get; set; }

    [Reactive]
    public bool IsCharacterThread { get; set; }

    [Reactive]
    public string TotalText { get; set; } = string.Empty;

    [Reactive]
    public string HookCode { get; init; } = string.Empty;

    public string EngineName { get; init; } = string.Empty;

    public long Context { get; set; }

    public long SubContext { get; init; }
}
