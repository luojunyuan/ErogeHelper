using System;
using System.Reactive;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.Share.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new();

        public Pages.GeneralViewModel GeneralViewModel { get; }
        public Pages.AboutViewModel AboutViewModel { get; }

        public PreferenceViewModel(
            IEHConfigRepository? ehConfigRepository = null)
        {
            GeneralViewModel = DependencyResolver.GetService<Pages.GeneralViewModel>();
            AboutViewModel = DependencyResolver.GetService<Pages.AboutViewModel>();
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
                    PageTag.About => Strings.About_Title,
                    _ => string.Empty
                });
        }

        [Reactive]
        public string PageHeader { get; set; }

        [Reactive]
        public double Height { get; set; }

        [Reactive]
        public double Width { get; set; }

        public ReactiveCommand<Unit, Unit> Closed { get; }

        private IObservable<Unit> SaveWindowSize(IEHConfigRepository ehConfigRepository)
        {
            return Observable.Start(() =>
            {
                ehConfigRepository.PreferenceWindowHeight = Height;
                ehConfigRepository.PreferenceWindowWidth = Width;
                return Unit.Default;
            });
        }
    }
}
