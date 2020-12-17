using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.ViewModels.Control;
using ErogeHelper.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsInput.Events;

namespace ErogeHelper.ViewModels
{
    class GameViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GameViewModel));

        private double fontSize = DataRepository.FontSize;
        private bool assistiveTouchIsVisible = true;

        public BindableCollection<string> AppendTextList { get; set; } = new BindableCollection<string>();

        public bool AssistiveTouchIsVisible
        { 
            get => assistiveTouchIsVisible;
            set 
            { 
                assistiveTouchIsVisible = value;
                NotifyOfPropertyChange(() => AssistiveTouchIsVisible);
            } 
        }

        public double FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                DataRepository.FontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }

        public void OpenPreference()
        {
            var window = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
            if (window == null)
                windowManager.ShowWindowAsync(IoC.Get<PreferenceViewModel>());
            else
                window.Activate();
        }

        public bool CanZoomIn() => true;
        public void ZoomIn()
        {
            FontSize += 2;
        }

        public bool CanZoomOut() => true;
        public void ZoomOut()
        {
            FontSize -= 2;
        }

        public bool CanTaskbarNotifyArea() => true;
        public async void TaskbarNotifyArea()
        {
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.A)
                .Invoke();
        }

        public bool CanTaskView() => true;
        public async void TaskView() => await WindowsInput.Simulate.Events().ClickChord(KeyCode.LWin, KeyCode.Tab).Invoke();

        public bool CanScreenShot() => true;
        public async void ScreenShot()
        {
            AssistiveTouchIsVisible = false;

            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Escape)
                .Invoke();
            
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
                .Invoke();

            await Task.Delay(3000);
            AssistiveTouchIsVisible = true;
        }

        public bool CanVolumeUp() => true;
        public async void VolumeUp() => await WindowsInput.Simulate.Events().Click(KeyCode.VolumeUp).Invoke();

        public bool CanSwitchGameScreen() => true;
        public async void SwitchGameScreen()
        {
            var handle = DataRepository.MainProcess!.MainWindowHandle;
            NativeMethods.BringWindowToTop(handle);
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.Alt, KeyCode.Enter)
                .Invoke();
        }


        public void PressKeyEnter()
        {
            int exStyle = NativeMethods.GetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_NOACTIVATE);

            // FIXmE: would make CommandBarFlyout lost focus, without it works good. maybe it's better to move Ctrl to another control
            // 游戏窗口得到focus，这边就会失去焦点，必须在他失去焦点之前就激活游戏窗口，所以还是换个控件吧 可以把窗口Focus开关switch放在GameView上，不放Preference了
            // 或者若可以直接向进程发送事件也不错 不好搞，还是弄个button，隐藏起来，让CommandBar面板把它打开, 把Alt+Enter也放在那边
            //var handle = DataRepository.MainProcess!.MainWindowHandle;
            //NativeMethods.BringWindowToTop(handle);
        }
        public async void PressSkip()
        {
            await WindowsInput.Simulate.Events()
                .Hold(KeyCode.Control)
                .Invoke();
        }
        public async void PressSkipRelease()
        {
            await WindowsInput.Simulate.Events()
                .Release(KeyCode.Control)
                .Invoke();
        }
        public void PressKeyLeave()
        {
            int exStyle = NativeMethods.GetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(DataRepository.GameViewHandle, NativeMethods.GWL_EXSTYLE, exStyle & ~NativeMethods.WS_EX_NOACTIVATE);
        }

        public TextViewModel TextControl { get; set; }
        readonly IWindowManager windowManager;
        private IGameViewDataService dataService;

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
