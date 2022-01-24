using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class DanmakuViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string? UrlPathSegment => PageTag.General;

    public DanmakuViewModel(IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        DanmakuEnable = ehConfigRepository.UseDanmaku;
        this.WhenAnyValue(x => x.DanmakuEnable)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseDanmaku = v);
    }

    [Reactive]
    public bool DanmakuEnable { get; set; }
}
