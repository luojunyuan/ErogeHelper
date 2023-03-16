using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function;
using ErogeHelper.Model.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class PreferenceViewModel : ReactiveObject, IScreen, IDisposable
{
    public RoutingState Router { get; } = new();

    public PreferenceViewModel(IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        PageHeader = string.Empty;

        Router.CurrentViewModel
            .WhereNotNull()
            .Select(x => x.UrlPathSegment)
            .WhereNotNull()
            .Subscribe(x => PageHeader = x switch
            {
                PreferencePageTag.General => Strings.GeneralPage_Title,
                PreferencePageTag.MeCab => Strings.MeCabPage_Title,
                PreferencePageTag.TTS => Strings.TTSPage_Title,
                PreferencePageTag.Trans => Strings.TransPage_Title,
                PreferencePageTag.About => Strings.About_Title,
                _ => string.Empty
            }).DisposeWith(_disposables);
    }

    [Reactive]
    public string PageHeader { get; set; }

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
