using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
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
                .Select(hp => hp.Text)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(LimitTextLength)
                .Subscribe(text => TotalText += "\n\n" + text).DisposeWith(d);
        });
    }

    private const int MaxLength = 1000;

    /// <summary>
    /// TextBox begin large and need rendering more, reduces GC pressure
    /// </summary>
    private void LimitTextLength(string obj)
    {
        if (TotalText.Length <= MaxLength) 
            return;
        
        var index = TotalText.LastIndexOf('\n', TotalText.Length - 1);
        foreach (var _ in Enumerable.Range(0, 3))
        {
            if (index < 2) break;
            index = TotalText.LastIndexOf('\n', index - 2);
        }
        if (index == -1) index = 0;
        TotalText = TotalText[index..];
    }

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

    public long Context { get; init; }

    public long SubContext { get; init; }
}
