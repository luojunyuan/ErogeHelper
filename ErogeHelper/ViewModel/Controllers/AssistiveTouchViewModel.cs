using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
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

            AssistiveTouchTemplate = GetAssistiveTouchStyle(_ehConfigRepositoy.UseBigAssistiveTouchSize);

            var interval = Observable
                .Interval(TimeSpan.FromMilliseconds(ConstantValues.GameWindowStatusRefreshTime))
                .TakeUntil(StayTopSubject.Where(on => !on));

            StayTopSubject
                .DistinctUntilChanged()
                .Where(on => on && !MainWindowHandle.IsNull)
                .SelectMany(interval)
                .Subscribe(_ => User32.BringWindowToTop(MainWindowHandle));

            _mainWindowDataService.AssistiveTouchBigSizeSubject
                .Subscribe(v => AssistiveTouchTemplate = GetAssistiveTouchStyle(v));

            TestCommand = ReactiveCommand.Create(() =>
            {
                var value = true;
                _ehConfigRepositoy.UseBigAssistiveTouchSize = value;
                _mainWindowDataService.AssistiveTouchBigSizeSubject.OnNext(value);
            });
        }

        private static ControlTemplate GetAssistiveTouchStyle(bool useBigSize) =>
            useBigSize ? Application.Current.Resources["BigAssistiveTouchTemplate"] as ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'BigAssistiveTouchTemplate'")
                       : Application.Current.Resources["NormalAssistiveTouchTemplate"] as ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'NormalAssistiveTouchTemplate'");

        [Reactive]
        public ControlTemplate AssistiveTouchTemplate { get; private set; }

        public ReactiveCommand<Unit, Unit> TestCommand { get; }
    }
}
