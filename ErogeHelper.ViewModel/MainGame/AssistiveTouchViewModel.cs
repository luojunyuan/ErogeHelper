using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.MainGame;

public class AssistiveTouchViewModel : ReactiveObject, IDisposable
{
    public AssistiveTouchViewModel(
        IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        ShowAssistiveTouch = true;
        AssistiveTouchPosition = JsonSerializer.Deserialize<AssistiveTouchPosition>
            (ehConfigRepository.AssistiveTouchPosition) ?? AssistiveTouchPosition.Default;

        this.WhenAnyValue(x => x.AssistiveTouchPosition)
            .Skip(1)
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(pos => ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(pos))
            .DisposeWith(_disposables);
    }

    [Reactive]
    public bool ShowAssistiveTouch { get; set; }

    [Reactive]
    public AssistiveTouchPosition AssistiveTouchPosition { get; set; }


    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
