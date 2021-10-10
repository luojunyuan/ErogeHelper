using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Windows;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
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
        private readonly IEhConfigRepository _ehConfigRepository;
        private readonly IGameDataService _gameDataService;
        private readonly IGameInfoRepository _ehDbRepository;
        private readonly IGameWindowHooker _gameWindowHooker;

        private readonly Subject<Unit> _hideFlyoutSubj = new();
        public IObservable<Unit> HideFlyoutSubj => _hideFlyoutSubj.AsObservable();

        public double ButtonSize => _ehConfigRepository.UseBigAssistiveTouchSize ?
            DefaultValues.AssistiveTouchBigSize :
            DefaultValues.AssistiveTouchSize;

        public AssistiveTouchPosition AssistiveTouchPosition
        {
            get => JsonSerializer.Deserialize<AssistiveTouchPosition>(_ehConfigRepository.AssistiveTouchPosition)
                ?? DefaultValues.TouchPosition;
            set => _ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(value);
        }

        public HWND MainWindowHandle => _mainWindowDataService.Handle;

        public ReplaySubject<double> DpiSubject => _mainWindowDataService.DpiSubject;

        public Subject<bool> AssistiveTouchBigSizeSubject => _mainWindowDataService.AssistiveTouchBigSizeSubject;

        public HWND GameWindowHandle => _gameDataService.MainWindowHandle;

        public IObservable<GameWindowPositionPacket> GamePosUpdated => _gameWindowHooker.GamePosUpdated;

        public AssistiveTouchViewModel(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            IGameDataService? gameDataService = null,
            IGameInfoRepository? ehDbRepository = null,
            ITouchConversionHooker? touchConversionHooker = null,
            IGameWindowHooker? gameWindowHooker = null)
        {
            _mainWindowDataService = mainWindowDataService ?? DependencyResolver.GetService<IMainWindowDataService>();
            _ehConfigRepository = ehConfigDataService ?? DependencyResolver.GetService<IEhConfigRepository>();
            _gameDataService = gameDataService ?? DependencyResolver.GetService<IGameDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyResolver.GetService<IGameInfoRepository>();
            touchConversionHooker ??= DependencyResolver.GetService<ITouchConversionHooker>();
            _gameWindowHooker = gameWindowHooker ?? DependencyResolver.GetService<IGameWindowHooker>();

#if !DEBUG // https://stackoverflow.com/questions/63723996/mouse-freezing-lagging-when-hit-breakpoint
            touchConversionHooker.Init();
#endif
            AssistiveTouchTemplate = GetAssistiveTouchStyle(_ehConfigRepository.UseBigAssistiveTouchSize);

            LoseFocusIsOn = _ehDbRepository.GameInfo!.IsLoseFocus;
            this.WhenAnyValue(x => x.LoseFocusIsOn)
                .Skip(1)
                .DistinctUntilChanged()
                .Subscribe(v =>
                {
                    Utils.WindowLostFocus(MainWindowHandle, v);
                    _ehDbRepository.UpdateLostFocusStatus(v);
                });

            IsTouchToMouse = _ehDbRepository.GameInfo!.IsEnableTouchToMouse;
            this.WhenAnyValue(x => x.IsTouchToMouse)
                .Skip(1)
                .Subscribe(v =>
                {
                    touchConversionHooker.Enable = v;
                    _ehDbRepository.UpdateTouchEnable(v);
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
                _hideFlyoutSubj.OnNext(Unit.Default);
                AssistiveTouchVisibility = Visibility.Collapsed;

                await WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                    .Invoke().ConfigureAwait(false);

                await Task.Delay(ConstantValues.ScreenShotHideButtonTime).ConfigureAwait(true);

                AssistiveTouchVisibility = Visibility.Visible;
            });

            OpenPreference = ReactiveCommand.Create(() =>
            {
                var window = Application.Current.Windows
                    .OfType<PreferenceWindow>()
                    .SingleOrDefault();
                if (window is null)
                {
                    DependencyResolver.ShowView<PreferenceViewModel>();
                }
                else
                {
                    window.Activate();
                }
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

        [Reactive]
        public bool IsTouchToMouse { get; set; }

        public ReactiveCommand<Unit, bool> VolumeDown { get; }
        public ReactiveCommand<Unit, bool> VolumeUp { get; }
        public ReactiveCommand<Unit, Unit> SwitchFullScreen { get; }

        public ReactiveCommand<Unit, bool> TaskbarNotifyArea { get; }
        public ReactiveCommand<Unit, bool> TaskView { get; }
        public ReactiveCommand<Unit, Unit> ScreenShot { get; }

        public ReactiveCommand<Unit, Unit> OpenPreference { get; }
    }
}
