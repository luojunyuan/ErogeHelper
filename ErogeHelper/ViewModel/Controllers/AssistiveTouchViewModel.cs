using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
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

            #region Updates When Fullscreen, WindowSize, DPI Changed 
            //BehaviorSubject<bool> _stayTopSubj = new(Utils.IsGameForegroundFullscreen(GameWindowHandle));

            //var interval = Observable
            //    .Interval(TimeSpan.FromMilliseconds(ConstantValues.GameWindowStatusRefreshTime))
            //    .TakeUntil(_stayTopSubj.Where(on => !on));

            //_stayTopSubj
            //    .DistinctUntilChanged()
            //    .Where(on => on && !MainWindowHandle.IsNull)
            //    .SelectMany(interval)
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(_ => User32.BringWindowToTop(MainWindowHandle));

            //_gameWindowHooker.GamePosUpdated
            //    .Where(_ => ParentFrameWorkElementLoaded)
            //    .Select(pos => (pos.Width, pos.Height))
            //    .DistinctUntilChanged()
            //    .Select(_ => Utils.IsGameForegroundFullscreen(GameWindowHandle))
            //    .Do(isFullscreen => _stayTopSubj.OnNext(isFullscreen))
            //    .Delay(TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime))
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(_ => _setButtonDockPosSubj.OnNext(Unit.Default)); 

            //DpiSubj
            //    .Where(_ => ParentFrameWorkElementLoaded)
            //    .Throttle(TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime))
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(_ => _setButtonDockPosSubj.OnNext(Unit.Default));
            #endregion

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

            _mainWindowDataService.AssistiveTouchBigSizeSubj
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
                AssistiveTouchVisibility = System.Windows.Visibility.Collapsed;

                await WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                    .Invoke().ConfigureAwait(false);

                await Task.Delay(ConstantValues.ScreenShotHideButtonTime).ConfigureAwait(true);

                AssistiveTouchVisibility = System.Windows.Visibility.Visible;
            });

            OpenPreference = ReactiveCommand.Create(() => DependencyResolver.ShowView<PreferenceViewModel>());
        }

        private readonly Subject<Unit> _setButtonDockPosSubj = new();
        public IObservable<Unit> SetButtonDockPosSubj => _setButtonDockPosSubj;

        private readonly Subject<Unit> _hideFlyoutSubj = new();
        public IObservable<Unit> HideFlyoutSubj => _hideFlyoutSubj;

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

        public ReplaySubject<double> DpiSubj { get; init; } = new(1);

        public Subject<bool> AssistiveTouchBigSizeSubject => _mainWindowDataService.AssistiveTouchBigSizeSubj;

        public HWND GameWindowHandle => _gameDataService.GameRealWindowHandle;

        public IObservable<GameWindowPositionPacket> GamePosUpdated => _gameWindowHooker.GamePosUpdated;

        public bool ParentFrameWorkElementLoaded { private get; set; }


        private static System.Windows.Controls.ControlTemplate GetAssistiveTouchStyle(bool useBigSize) =>
            useBigSize ? System.Windows.Application.Current.Resources["BigAssistiveTouchTemplate"] 
                            as System.Windows.Controls.ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'BigAssistiveTouchTemplate'")
                       : System.Windows.Application.Current.Resources["NormalAssistiveTouchTemplate"] 
                            as System.Windows.Controls.ControlTemplate
                            ?? throw new IOException("Cannot locate resource 'NormalAssistiveTouchTemplate'");

        [Reactive]
        public System.Windows.Controls.ControlTemplate AssistiveTouchTemplate { get; private set; }

        [Reactive]
        public System.Windows.Visibility AssistiveTouchVisibility { get; private set; }


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
