using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
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

            var currentScreen = WpfScreenHelper.Screen.FromHandle(mainWindowDataService.Handle.DangerousGetHandle());
            Left = currentScreen.PixelBounds.Left + ((currentScreen.PixelBounds.Width - (Width * currentScreen.ScaleFactor)) / 2);
            Top = currentScreen.PixelBounds.Top + ((currentScreen.PixelBounds.Height - (Height * currentScreen.ScaleFactor)) / 2);

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

        [Reactive]
        public double Left { get; set; }

        [Reactive]
        public double Top { get; set; }

        public ReactiveCommand<Unit, Unit> Loaded { get; init; }
        public ReactiveCommand<Unit, Unit> Closed { get; init; }
    }
}
