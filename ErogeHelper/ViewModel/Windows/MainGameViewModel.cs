using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Windows
{
    public class MainGameViewModel : ReactiveObject, IEnableLogger
    {
        private double _dpi;

        public AssistiveTouchViewModel AssistiveTouchViewModel { get; }

        public MainGameViewModel(
            AssistiveTouchViewModel? assistiveTouchViewModel = null,
            IEhConfigRepository? ehConfigRepository = null,
            IMainWindowDataService? mainWindowDataService = null,
            IGameWindowHooker? gameWindowHooker = null,
            IEhDbRepository? ehDbRepository = null)
        {
            AssistiveTouchViewModel = assistiveTouchViewModel ?? DependencyInject.GetService<AssistiveTouchViewModel>();
            gameWindowHooker ??= DependencyInject.GetService<IGameWindowHooker>();
            mainWindowDataService ??= DependencyInject.GetService<IMainWindowDataService>();
            ehConfigRepository ??= DependencyInject.GetService<IEhConfigRepository>();
            ehDbRepository ??= DependencyInject.GetService<IEhDbRepository>();

            _dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow).DpiScaleX;

            gameWindowHooker.GamePosUpdated
                .Subscribe(pos =>
                {
                    Height = pos.Height / _dpi;
                    Width = pos.Width / _dpi;
                    Left = pos.Left / _dpi;
                    Top = pos.Top / _dpi;
                    ClientAreaMargin = new Thickness(
                        pos.ClientArea.Left / _dpi,
                        pos.ClientArea.Top / _dpi,
                        pos.ClientArea.Right / _dpi,
                        pos.ClientArea.Bottom / _dpi);
                });
            gameWindowHooker.InvokeUpdatePosition();

            UseEdgeTouchMask = ehConfigRepository.UseEdgeTouchMask;

            Loaded = ReactiveCommand.Create(() =>
            {
                mainWindowDataService.SetHandle(MainWindowHandle);
                Utils.HideWindowInAltTab(MainWindowHandle);
                gameWindowHooker.InvokeUpdatePosition();

                if (ehDbRepository.GameInfo!.IsLoseFocus)
                {
                    Utils.WindowLostFocus(MainWindowHandle, ehDbRepository.GameInfo!.IsLoseFocus);
                }
            });

            DpiChanged = ReactiveCommand.Create<double>(dpi =>
            {
                this.Log().Debug($"Current screen dpi {dpi * 100}%");
                _dpi = dpi;
                mainWindowDataService.DpiSubject.OnNext(dpi);
                gameWindowHooker.InvokeUpdatePosition();
            });
        }

        public HWND MainWindowHandle { get; set; }

        [Reactive]
        public double Height { get; private set; }

        [Reactive]
        public double Width { get; private set; }

        [Reactive]
        public double Left { get; private set; }

        [Reactive]
        public double Top { get; private set; }

        [Reactive]
        public Thickness ClientAreaMargin { get; private set; }

        [Reactive]
        public bool UseEdgeTouchMask { get; set; }

        public ReactiveCommand<Unit, Unit> Loaded { get; init; }

        public ReactiveCommand<double, Unit> DpiChanged { get; init; }
    }
}
