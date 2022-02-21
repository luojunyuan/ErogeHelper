using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class PreferenceViewModel : ReactiveObject, IScreen, IDisposable
{
    public RoutingState Router { get; } = new();

    public PreferenceViewModel(
        IEHConfigRepository? ehConfigRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();

        PageHeader = string.Empty;
        Height = ehConfigRepository.PreferenceWindowHeight;
        Width = ehConfigRepository.PreferenceWindowWidth;

        Closed = ReactiveCommand.CreateFromObservable(() => SaveWindowSize(ehConfigRepository));

        Router.CurrentViewModel
            .WhereNotNull()
            .Select(x => x.UrlPathSegment)
            .WhereNotNull()
            .Subscribe(x => PageHeader = x switch
            {
                PageTag.General => Strings.GeneralPage_Title,
                PageTag.MeCab => Strings.MeCabPage_Title,
                PageTag.TTS => Strings.TTSPage_Title,
                PageTag.Trans => Strings.TransPage_Title,
                PageTag.About => Strings.About_Title,
                _ => string.Empty
            }).DisposeWith(_disposables);
    }

    [Reactive]
    public string PageHeader { get; set; }

    [Reactive]
    public double Height { get; set; }

    [Reactive]
    public double Width { get; set; }

    public ReactiveCommand<Unit, Unit> Closed { get; }

    private IObservable<Unit> SaveWindowSize(IEHConfigRepository ehConfigRepository) =>
        Observable.Start(() =>
        {
            ehConfigRepository.PreferenceWindowHeight = Height;
            ehConfigRepository.PreferenceWindowWidth = Width;
            return Unit.Default;
        });

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
