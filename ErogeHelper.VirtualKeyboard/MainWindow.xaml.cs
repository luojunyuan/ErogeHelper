using System;
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

            SetWindowPosition();

            Loaded += (s, e) => KeyTrrricksters.Load(TouchToolBoxView);
        }

        private void SetWindowPosition()
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
            Left = rectDpi.Left; Top = rectDpi.Top;
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
                SetWindowPosition();
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
    }
}
