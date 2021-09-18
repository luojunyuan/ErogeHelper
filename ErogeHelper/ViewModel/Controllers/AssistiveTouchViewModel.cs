using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.IO;
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
        private readonly IEhDbRepository _ehDbRepository;

        private readonly Subject<bool> _hideFlyoutSubj = new();
        public IObservable<bool> HideFlyoutSubj => _hideFlyoutSubj.AsObservable();

        private HWND MainWindowHandle => _mainWindowDataService.Handle;

        public AssistiveTouchViewModel(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            IGameDataService? gameDataService = null,
            IEhDbRepository? ehDbRepository = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            _ehConfigRepositoy = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();

            AssistiveTouchTemplate = GetAssistiveTouchStyle(_ehConfigRepositoy.UseBigAssistiveTouchSize);

            LoseFocusIsOn = _ehDbRepository.GameInfo!.IsLoseFocus;
            this.WhenAnyValue(x => x.LoseFocusIsOn)
                .Skip(1)
                .Subscribe(v =>
                {
                    Utils.WindowLostFocus(MainWindowHandle, v);
                    _ehDbRepository.UpdateLostFocusStatus(v);
                });

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
                // Tip: If a window's main handle not the Process.MainWindowHandle, so the Alt+Enter also not work
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
                _hideFlyoutSubj.OnNext(true);
                AssistiveTouchVisibility = Visibility.Collapsed;

                await WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                    .Invoke().ConfigureAwait(false);

                await Task.Delay(ConstantValues.ScreenShotHideButtonTime).ConfigureAwait(true);

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

        [Reactive]
        public bool LoseFocusIsOn { get; set; }

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

        private void ChangeAssistiveTouchSize(bool bigStyle)
        {
            _ehConfigRepositoy.UseBigAssistiveTouchSize = bigStyle;
            _mainWindowDataService.AssistiveTouchBigSizeSubject.OnNext(bigStyle);
        }
    }
}
