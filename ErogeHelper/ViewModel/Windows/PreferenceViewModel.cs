using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IPreferenceScreen, IEnableLogger
    {
        public RoutingState Router { get; } = new();

        public PreferenceViewModel(IEhDbRepository? ehDbRepository = null, IEhConfigRepository? ehConfigRepository = null)
        {
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

            ehDbRepository ??= DependencyInject.GetService<IEhDbRepository>();
            ehConfigRepository ??= DependencyInject.GetService<IEhConfigRepository>();

            Height = ehConfigRepository.PreferenceWindowHeight;
            Width = ehConfigRepository.PreferenceWindowWidth;

            Loaded = ReactiveCommand.Create(() =>
            {
                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(PreferenceWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });
            Closed = ReactiveCommand.Create(() =>
            {
                ehConfigRepository.PreferenceWindowHeight = Height;
                ehConfigRepository.PreferenceWindowWidth = Width;
            });
        }

        public HWND PreferenceWindowHandle { get; set; }

        [Reactive]
        public string PageHeader { get; set; } = string.Empty;

        [Reactive]
        public double Height { get; set; }

        [Reactive]
        public double Width { get; set; }

        public ReactiveCommand<Unit, Unit> Loaded { get; init; }
        public ReactiveCommand<Unit, Unit> Closed { get; init; }
    }
}
