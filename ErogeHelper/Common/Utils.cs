using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?.?";

        public static void HideWindowInAltTab(Window window)
        {
            const int WsExToolWindow = 0x00000080;

            var windowInterop = new WindowInteropHelper(window);
            var exStyle = User32.GetWindowLong(new HWND(windowInterop.Handle), 
                                               User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= WsExToolWindow;
            _ = User32.SetWindowLong(new HWND(windowInterop.Handle), User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }

        public static void HideWindowInAltTab(IntPtr windowHandle)
        {
            const int WsExToolWindow = 0x00000080;

            var exStyle = User32.GetWindowLong(new HWND(windowHandle), 
                                               User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= WsExToolWindow;
            _ = User32.SetWindowLong(new HWND(windowHandle), User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }
    }
}
