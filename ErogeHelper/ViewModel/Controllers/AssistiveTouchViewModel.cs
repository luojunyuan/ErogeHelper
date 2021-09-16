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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Vanara.PInvoke;
using WindowsInput.Events;

namespace ErogeHelper.ViewModel.Controllers
{
    public class AssistiveTouchViewModel : ReactiveObject, IEnableLogger
    {
        private readonly IMainWindowDataService _mainWindowDataService;
        private readonly IEhConfigRepository _ehConfigRepositoy;
        private readonly IGameDataService _gameDataService;
        private HWND MainWindowHandle => _mainWindowDataService.Handle;
        private readonly BehaviorSubject<bool> StayTopSubject = new(false);

        public AssistiveTouchViewModel(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            IGameDataService? gameDataService = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            _ehConfigRepositoy = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();

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


            VolumeDown = ReactiveCommand.CreateFromTask(async () =>
                await WindowsInput.Simulate.Events()
                        .Click(KeyCode.VolumeDown)
                        .Invoke().ConfigureAwait(false));
            VolumeUp = ReactiveCommand.CreateFromTask(async () =>
                await WindowsInput.Simulate.Events()
                        .Click(KeyCode.VolumeUp)
                        .Invoke().ConfigureAwait(false));
            SwitchFullScreen = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: may use true handle instead
                User32.BringWindowToTop(_gameDataService.MainProcess.MainWindowHandle);
                await WindowsInput.Simulate.Events()
                        .ClickChord(KeyCode.Alt, KeyCode.Enter)
                        .Invoke().ConfigureAwait(false);
            });

            TaskbarNotifyArea = ReactiveCommand.CreateFromTask(async () =>
                await WindowsInput.Simulate.Events()
                        .ClickChord(KeyCode.LWin, KeyCode.A)
                        .Invoke().ConfigureAwait(false));
            TaskView = ReactiveCommand.CreateFromTask(async () =>
                await WindowsInput.Simulate.Events()
                        .ClickChord(KeyCode.LWin, KeyCode.Tab)
                        .Invoke().ConfigureAwait(false));
            ScreenShot = ReactiveCommand.CreateFromTask(async () =>
            {
                AssistiveTouchVisibility = Visibility.Collapsed;

                await WindowsInput.Simulate.Events()
                    .Click(KeyCode.Escape)
                    .Invoke().ConfigureAwait(false);

                // Wait for CommandBarFlyout hide
                await Task.Delay(500).ConfigureAwait(false);

                await WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                    .Invoke().ConfigureAwait(false);

                await Task.Delay(3000).ConfigureAwait(false);

                AssistiveTouchVisibility = Visibility.Visible;
            });
        }

        private static ControlTemplate GetAssistiveTouchStyle(bool useBigSize) =>
            useBigSize ? Application.Current.Resources["BigAssistiveTouchTemplate"] as ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'BigAssistiveTouchTemplate'")
                       : Application.Current.Resources["NormalAssistiveTouchTemplate"] as ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'NormalAssistiveTouchTemplate'");

        [Reactive]
        public ControlTemplate AssistiveTouchTemplate { get; private set; }

        [Reactive]
        public Visibility AssistiveTouchVisibility { get; private set; }

        public ReactiveCommand<Unit, Unit> ZoomOut { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, Unit> ZoomIn { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, bool> VolumeDown { get; }
        public ReactiveCommand<Unit, bool> VolumeUp { get; }
        public ReactiveCommand<Unit, Unit> SwitchFullScreen { get; }
        public ReactiveCommand<Unit, Unit> FocusToggle { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, Unit> TouchToMouseToggle { get; } = ReactiveCommand.Create(() => { });

        public ReactiveCommand<Unit, bool> TaskbarNotifyArea { get; }
        public ReactiveCommand<Unit, bool> TaskView { get; }
        public ReactiveCommand<Unit, Unit> ScreenShot { get; }

        public ReactiveCommand<Unit, Unit> OpenPreference { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, Unit> PressSkip { get; } = ReactiveCommand.Create(() => { });
        public ReactiveCommand<Unit, Unit> PressSkipRelease { get; } = ReactiveCommand.Create(() => { });

        private void ChangeAssistiveTouchSize()
        {
            var value = true;
            _ehConfigRepositoy.UseBigAssistiveTouchSize = value;
            _mainWindowDataService.AssistiveTouchBigSizeSubject.OnNext(value);
        }
    }
}
