using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WindowsInput.Events;

namespace ErogeHelper.VirtualKeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const uint EventObjectLocationChange = 0x800B;
        private readonly GCHandle _gcSafetyHandle;
        private readonly IntPtr _windowsEventHook;

        private readonly double Dpi;
        private const int ButtonWidth = 75;

        public MainWindow()
        {
            InitializeComponent();
            Dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            var handle = new WindowInteropHelper(this).EnsureHandle();
            HideWindowInAltTab(handle);
            WindowLostFocus(handle, true);

            var targetThreadId = User32.GetWindowThreadProcessId(App.GameWindowHandle, out var pid);

            User32.WinEventProc winEventDelegate = WinEventCallback;
            _gcSafetyHandle = GCHandle.Alloc(winEventDelegate);

            _windowsEventHook = User32.SetWinEventHook(
                 EventObjectLocationChange, EventObjectLocationChange,
                 IntPtr.Zero, winEventDelegate, pid, targetThreadId,
                 0 | 2);

            Closed += MainWindow_Closed;

            SetWindow();
        }

        private void SetWindow()
        {
            // ATTENTION: User32.RECT is different with System.Drawing.Rectangle
            User32.GetWindowRect(App.GameWindowHandle, out var rect);
            User32.GetClientRect(App.GameWindowHandle, out var rectClient);
            // rect.Right - rect.Left == rect.Width == (0, 0) to client right-bottom point 
            var rectWidth = rect.Width - rect.Left;
            var rectHeight = rect.Height - rect.Top;

            var winShadow = (rectWidth - rectClient.Width) / 2;
            var left = rect.Left + winShadow;

            var winTitleHeight = rectHeight - rectClient.Height - winShadow;
            var top = rect.Top + winTitleHeight;

            var rectDpi = new Rectangle((int)(left / Dpi), (int)(top / Dpi), (int)(rectClient.Width / Dpi), (int)(rectClient.Height / Dpi));
            Left = rectDpi.Left; Top = rectDpi.Top ;
            Width = rectDpi.Width; Height = rectDpi.Height;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _gcSafetyHandle.Free();
            User32.UnhookWinEvent(_windowsEventHook);
        }

        private void WinEventCallback(User32.HWINEVENTHOOK hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EventObjectLocationChange &&
                hWnd == App.GameWindowHandle &&
                idObject == 0 && idChild == 0)
            {
                SetWindow();
            }
        }

        private static void HideWindowInAltTab(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
                return;

            const int wsExToolWindow = 0x00000080;

            var exStyle = User32.GetWindowLong(windowHandle,
                User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= wsExToolWindow;
            _ = User32.SetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }

        private static void WindowLostFocus(IntPtr windowHandle, bool loseFocus)
        {
            if (windowHandle == IntPtr.Zero)
                return;

            var exStyle = User32.GetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE);
            if (loseFocus)
            {
                User32.SetWindowLong(windowHandle,
                    User32.WindowLongFlags.GWL_EXSTYLE,
                    exStyle | (int)User32.WindowStylesEx.WS_EX_NOACTIVATE);
            }
            else
            {
                User32.SetWindowLong(windowHandle,
                    User32.WindowLongFlags.GWL_EXSTYLE,
                    exStyle & ~(int)User32.WindowStylesEx.WS_EX_NOACTIVATE);
            }
        }

        private void PanelControlButtonOnClick(object sender, RoutedEventArgs e)
        {
            TheButtonBox.SetCurrentValue(VisibilityProperty,
                TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            PanelControlButton.SetCurrentValue(ContentProperty, PanelControlButton.Content.ToString() == "<" ? '>' : '<');
        }

        private const int UserTimerMinimum = 0x0000000A;
        private const int UIMinimumResponseTime = 50;

        private static EventBuilder PressKeyWithDelay(KeyCode key) =>
            WindowsInput.Simulate.Events()
                .Hold(key)
                .Wait(UserTimerMinimum)
                .Release(key);

        private async void Esc(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Escape).Invoke().ConfigureAwait(false);

        private async void Ctrl(object sender, MouseButtonEventArgs e) =>
            await WindowsInput.Simulate.Events().Hold(KeyCode.Control).Invoke().ConfigureAwait(false);

        private async void CtrlRelease(object sender, MouseButtonEventArgs e) =>
            await WindowsInput.Simulate.Events().Release(KeyCode.Control).Invoke().ConfigureAwait(false);

        private async void Enter(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Enter).Invoke().ConfigureAwait(false);

        private async void Space(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Space).Invoke().ConfigureAwait(false);

        private async void PageUp(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.PageUp).Invoke().ConfigureAwait(false);

        private async void PageDown(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.PageDown).Invoke().ConfigureAwait(false);

        private async void UpArrow(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Up).Invoke().ConfigureAwait(false);

        private async void LeftArrow(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Left).Invoke().ConfigureAwait(false);

        private async void DownArrow(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Down).Invoke().ConfigureAwait(false);

        private async void RightArrow(object sender, RoutedEventArgs e) =>
            await PressKeyWithDelay(KeyCode.Right).Invoke().ConfigureAwait(false);

        private async void ScrollUpOnClick(object sender, RoutedEventArgs e)
        {
            ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            // Certain focus by mouse position
            await WindowsInput.Simulate.Events()
                .Wait(UIMinimumResponseTime)
                .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Up)
                .Wait(UIMinimumResponseTime)
                .Invoke().ConfigureAwait(true);
            ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        private async void ScrollDownOnClick(object sender, RoutedEventArgs e)
        {
            ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            await WindowsInput.Simulate.Events()
                .Wait(UIMinimumResponseTime)
                .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Down)
                .Wait(UIMinimumResponseTime)
                .Invoke().ConfigureAwait(true);
            ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        private void CloseKeyboardButtonOnClick(object sender, RoutedEventArgs e) => Close();
    }

}
