using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Controllers
{
    public class AssistiveTouchViewModel : ReactiveObject, IEnableLogger
    {
        private readonly IMainWindowDataService _mainWindowDataService;
        private readonly IEhConfigRepository _ehConfigRepositoy;
        private HWND MainWindowHandle => _mainWindowDataService.Handle;
        private readonly BehaviorSubject<bool> StayTopSubject = new(false);

        public AssistiveTouchViewModel(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            _ehConfigRepositoy = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();

            //UpdateAssistiveTouchSize(_ehConfigRepositoy.AssistiveTouchSize);

            var interval = Observable
                .Interval(TimeSpan.FromMilliseconds(ConstantValues.GameWindowStatusRefreshTime))
                .TakeUntil(StayTopSubject.Where(on => !on));

            StayTopSubject
                .DistinctUntilChanged()
                .Where(on => on && !MainWindowHandle.IsNull)
                .SelectMany(interval)
                .Subscribe(_ => User32.BringWindowToTop(MainWindowHandle));

            TestCommand = ReactiveCommand.Create(() =>
            {
                //UpdateAssistiveTouchSize(ConstantValues.AssistiveTouchBigSize);
            });
        }

        //[Reactive]
        //public double AssistiveTouchSize { get; private set; }

        public ReactiveCommand<Unit, Unit> TestCommand { get; }

        private void UpdateAssistiveTouchSize(double size)
        {
            //AssistiveTouchSize = size;
            //Application.Current.Resources["AssistiveTouchLayerZeroCornerRadius"] = new CornerRadius(size / 4);
            //Application.Current.Resources["AssistiveTouchLayerOneMargin"] = new Thickness(size / 8);
            //Application.Current.Resources["AssistiveTouchLayerTwoMargin"] = new Thickness((size / 8) + (size / 16));
            //Application.Current.Resources["AssistiveTouchLayerThreeMargin"] = new Thickness(size / 4);
        }
    }
}
