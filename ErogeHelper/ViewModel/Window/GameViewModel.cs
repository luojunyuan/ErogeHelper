using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
using ErogeHelper.View.Window;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput.Events;

namespace ErogeHelper.ViewModel.Window
{
    public class GameViewModel : PropertyChangedBase
    {

        public GameViewModel(
            IWindowManager windowManager,
            EhConfigRepository ehConfigRepository,
            EhGlobalValueRepository ehGlobalValueRepository)
        {
            _windowManager = windowManager;
            _ehConfigRepository = ehConfigRepository;
            _ehGlobalValueRepository = ehGlobalValueRepository;

            _fontSize = _ehConfigRepository.FontSize;
        }

        private readonly IWindowManager _windowManager;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly EhGlobalValueRepository _ehGlobalValueRepository;

        public BindableCollection<AppendTextItem> AppendTextList { get; set; } = new();

        // UNDONE: OutsideView scroll able text
        public ConcurrentCircularBuffer<string> SourceTextArchiver = new(30);

        private bool _assistiveTouchIsVisible = true;
        public bool AssistiveTouchIsVisible
        {
            get => _assistiveTouchIsVisible;
            set { _assistiveTouchIsVisible = value; NotifyOfPropertyChange(() => AssistiveTouchIsVisible); }
        }

        private double _fontSize;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _ehConfigRepository.FontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }

        public bool CanZoomIn => true;
        public void ZoomIn()
        {
            FontSize += 2;
            NotifyOfPropertyChange(() => CanZoomOut);
        }

        public bool CanZoomOut => FontSize > 3;
        public void ZoomOut()
        {
            FontSize -= 2;
            NotifyOfPropertyChange(() => CanZoomOut);
        }

        public bool CanVolumeUp => true;
        public async void VolumeUp() => await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeUp).Invoke().ConfigureAwait(false);
        public bool CanVolumeDown => true;
        public async void VolumeDown() => await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeDown).Invoke().ConfigureAwait(false);

        public bool CanSwitchGameScreen => true;
        public async void SwitchGameScreen()
        {
            var handle = _ehGlobalValueRepository.MainProcess.MainWindowHandle;
            NativeMethods.BringWindowToTop(handle);
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.Alt, KeyCode.Enter)
                .Invoke()
                .ConfigureAwait(false);
        }

        #region TextControl Pin

        private Visibility _pinSourceTextToggleVisibility;
        public Visibility PinSourceTextToggleVisibility
        {
            get => _pinSourceTextToggleVisibility;
            set
            {
                _pinSourceTextToggleVisibility = value;
                NotifyOfPropertyChange(() => PinSourceTextToggleVisibility);
            }
        }

        private bool _isSourceTextPined = true;
        public bool IsSourceTextPined
        {
            get => _isSourceTextPined;
            set { _isSourceTextPined = value; NotifyOfPropertyChange(() => IsSourceTextPined); }
        }

        public bool CanPinSourceTextToggle => true;
        public void PinSourceTextToggle()
        {
            if (IsSourceTextPined)
            {
                TriggerBarVisibility = Visibility.Collapsed;
                TextControlVisibility = Visibility.Visible;
                //TextControl.Background = new SolidColorBrush();
            }
            else
            {
                TriggerBarVisibility = Visibility.Visible;
                TextControlVisibility = Visibility.Collapsed;
                //TextControl.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
            }
        }

        private Visibility _triggerBarVisibility = Visibility.Collapsed;
        public Visibility TriggerBarVisibility
        {
            get => _triggerBarVisibility;
            set
            {
                _triggerBarVisibility = value;
                NotifyOfPropertyChange(() => TriggerBarVisibility);
            }
        }

        private Visibility _textControlVisibility = Visibility.Visible;

        public Visibility TextControlVisibility
        {
            get => _textControlVisibility;
            set
            {
                _textControlVisibility = value;
                NotifyOfPropertyChange(() => TextControlVisibility);
            }
        }

        public void TriggerBarEnter()
        {
            if (IsSourceTextPined == false)
            {
                TextControlVisibility = Visibility.Visible;
                TriggerBarVisibility = Visibility.Collapsed;
            }
        }

        public void TextControlLeave()
        {
            if (IsSourceTextPined == false)
            {
                TextControlVisibility = Visibility.Collapsed;
                TriggerBarVisibility = Visibility.Visible;
            }
        }

        #endregion

        // UNDONE: Use GameInfoTable to save single game config
        private bool _isLostFocus;

        public bool IsLostFocus
        {
            get => _isLostFocus;
            set { _isLostFocus = value; NotifyOfPropertyChange(() => IsLostFocus); }
        }
        public void FocusToggle()
        {
            // UNDONE: Use message 
            if (IsLostFocus)
            {
                var exStyle = NativeMethods.GetWindowLong(
                                                            _ehGlobalValueRepository.GameInsideViewHandle,
                                                            NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(
                                            _ehGlobalValueRepository.GameInsideViewHandle,
                                            NativeMethods.GWL_EXSTYLE,
                                            exStyle | NativeMethods.WS_EX_NOACTIVATE);

                //GameConfig.NoFocus = true;
            }
            else
            {
                var exStyle = NativeMethods.GetWindowLong(
                                                        _ehGlobalValueRepository.GameInsideViewHandle,
                                                        NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(
                                            _ehGlobalValueRepository.GameInsideViewHandle,
                                            NativeMethods.GWL_EXSTYLE,
                                            exStyle & ~NativeMethods.WS_EX_NOACTIVATE);

                //GameConfig.NoFocus = false;
            }
        }

        public bool CanTaskbarNotifyArea => true;
        public async void TaskbarNotifyArea() => await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.A).Invoke().ConfigureAwait(false);

        public bool CanTaskView => true;
        public async void TaskView() => await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab).Invoke().ConfigureAwait(false);

        public bool CanScreenShot => true;
        public async void ScreenShot()
        {
            AssistiveTouchIsVisible = false;

            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Escape).Invoke().ConfigureAwait(false);

            // Wait for CommandBarFlyout hide
            await Task.Delay(500).ConfigureAwait(false);

            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S).Invoke().ConfigureAwait(false);

            await Task.Delay(3000).ConfigureAwait(false);

            AssistiveTouchIsVisible = true;
        }

        public async void OpenPreference()
        {
            var window = Application.Current.Windows.OfType<PreferenceView>().SingleOrDefault();
            if (window is null)
            {
                await _windowManager.ShowWindowFromIoCAsync<PreferenceViewModel>().ConfigureAwait(false);
            }
            else
            {
                window.Activate();
            }
        }

        public async void PressSkip() => await WindowsInput.Simulate.Events()
            .Hold(KeyCode.Control).Invoke().ConfigureAwait(false);
        public async void PressSkipRelease() => await WindowsInput.Simulate.Events()
            .Release(KeyCode.Control).Invoke().ConfigureAwait(false);
    }
}