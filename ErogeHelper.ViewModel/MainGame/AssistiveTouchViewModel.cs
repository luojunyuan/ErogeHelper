using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
    public AssistiveTouchViewModel(IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        AssistiveTouchPosition = JsonSerializer.Deserialize<AssistiveTouchPosition>
            (ehConfigRepository.AssistiveTouchPosition) ?? AssistiveTouchPosition.Default;

        this.WhenAnyValue(x => x.AssistiveTouchPosition)
            .Skip(1)
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            // FIXME: System.IO.IOException:“无法将替换文件移到要被替换的文件。要被替换的文件保持原始名称。”
            .Subscribe(pos => ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(pos))
            .DisposeWith(_disposables);

        _useBigSizeSubj = new(ehConfigRepository.UseBigAssistiveTouchSize);

        ehConfigRepository.WhenAnyValue(x => x.UseBigAssistiveTouchSize)
            .Skip(1)
            .Subscribe(useBigSize => _useBigSizeSubj.OnNext(useBigSize))
            .DisposeWith(_disposables);
    }

    [Reactive]
    public AssistiveTouchPosition AssistiveTouchPosition { get; set; }

    private readonly BehaviorSubject<bool> _useBigSizeSubj;
    public IObservable<bool> UseBigSize => _useBigSizeSubj;

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
