using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IScreen, IEnableLogger
    {
        public RoutingState Router { get; } = new();

        public PreferenceViewModel(
            IGameInfoRepository? ehDbRepository = null,
            IEhConfigRepository? ehConfigRepository = null,
            IGameWindowHooker? gameWindowHooker = null,
            IMainWindowDataService? mainWindowDataService = null)
        {
            ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();
            gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
            mainWindowDataService ??= DependencyResolver.GetService<IMainWindowDataService>();

            Router.CurrentViewModel
                .WhereNotNull()
                .Select(x => x.UrlPathSegment)
                .WhereNotNull()
                .Subscribe(x => PageHeader = x switch
                {
                    PageTags.General => Language.Strings.GeneralPage_Title,
                    PageTags.About => Language.Strings.About_Title,
                    _ => string.Empty
                });

            Height = ehConfigRepository.PreferenceWindowHeight;
            Width = ehConfigRepository.PreferenceWindowWidth;

            ClosedCommand = ReactiveCommand.Create(() =>
            {
                ehConfigRepository.PreferenceWindowHeight = Height;
                ehConfigRepository.PreferenceWindowWidth = Width;
            });
        }

        [Reactive]
        public string PageHeader { get; set; } = string.Empty;

        [Reactive]
        public double Height { get; set; }

        [Reactive]
        public double Width { get; set; }

        public ReactiveCommand<Unit, Unit> ClosedCommand { get; init; }
    }
}
