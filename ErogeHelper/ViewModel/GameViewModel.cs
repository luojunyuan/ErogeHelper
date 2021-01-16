using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using ErogeHelper.View;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsInput.Events;

namespace ErogeHelper.ViewModel
{
    class GameViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GameViewModel));

        #region Properties
        private double _fontSize = DataRepository.FontSize;
        private bool _assistiveTouchIsVisible = true;
        #endregion

        public BindableCollection<string> AppendTextList { get; set; } = new BindableCollection<string>();

        public bool AssistiveTouchIsVisible
        {
            get => _assistiveTouchIsVisible;
            set
            {
                _assistiveTouchIsVisible = value;
                NotifyOfPropertyChange(() => AssistiveTouchIsVisible);
            }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                DataRepository.FontSize = value;
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
        public async void VolumeUp() => await WindowsInput.Simulate.Events().Click(KeyCode.VolumeUp).Invoke().ConfigureAwait(false);
        public bool CanVolumeDown => true;
        public async void VolumeDown() => await WindowsInput.Simulate.Events().Click(KeyCode.VolumeDown).Invoke().ConfigureAwait(false);

        public bool CanSwitchGameScreen => true;
        public async void SwitchGameScreen()
        {
            var handle = DataRepository.MainProcess!.MainWindowHandle;
            NativeMethods.BringWindowToTop(handle);
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.Alt, KeyCode.Enter)
                .Invoke()
                .ConfigureAwait(false);
        }

        private bool isLostFocus = GameConfig.NoFocus;
        public bool IsLostFocus { get => isLostFocus; set { isLostFocus = value; NotifyOfPropertyChange(() => IsLostFocus); } }
        public void FocusToggle()
        {
            if (IsLostFocus)
            {
                int exStyle = NativeMethods.GetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_NOACTIVATE);

                GameConfig.NoFocus = true;
                GameConfig.SetValue(EHNode.NoFocus, true.ToString());
            }
            else
            {
                int exStyle = NativeMethods.GetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE, exStyle & ~NativeMethods.WS_EX_NOACTIVATE);

                GameConfig.NoFocus = false;
                GameConfig.SetValue(EHNode.NoFocus, false.ToString());
            }
        }

        public bool CanTaskbarNotifyArea => true;
        public async void TaskbarNotifyArea()
        {
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.A)
                .Invoke()
                .ConfigureAwait(false);
        }

        public bool CanTaskView => true;
        public async void TaskView() => await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab).Invoke().ConfigureAwait(false);

        public bool CanScreenShot => true;
        public async void ScreenShot()
        {
            AssistiveTouchIsVisible = false;

            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Escape)
                .Invoke()
                .ConfigureAwait(false);

            // Wait for CommandBarFlyout hide
            await Task.Delay(500).ConfigureAwait(false);

            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                .Invoke()
                .ConfigureAwait(false);

            await Task.Delay(3000).ConfigureAwait(false);

            AssistiveTouchIsVisible = true;
        }

        public async void OpenPreference()
        {
            var window = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
            if (window == null)
                await windowManager.ShowWindowAsync(IoC.Get<PreferenceViewModel>()).ConfigureAwait(false);
            else
                window.Activate();
        }

        public async void PressSkip()
        {
            await WindowsInput.Simulate.Events()
                .Hold(KeyCode.Control)
                .Invoke()
                .ConfigureAwait(false);
        }
        public async void PressSkipRelease()
        {
            await WindowsInput.Simulate.Events()
                .Release(KeyCode.Control)
                .Invoke()
                .ConfigureAwait(false);
        }

        public TextViewModel TextControl { get; set; }
        readonly IWindowManager windowManager;
        private readonly IGameViewDataService dataService;

        public GameViewModel(
            IWindowManager windowManager,
            IGameViewDataService dataService,
            TextViewModel textControl)
        {
            this.dataService = dataService;
            this.windowManager = windowManager;
            TextControl = textControl;

            dataService.Start();
            dataService.SourceDataEvent += (_, receiveData) => TextControl.SourceTextCollection = receiveData;
            dataService.AppendDataEvent += (_, receiveData) => AppendTextList.Add(receiveData);
        }
    }
}
