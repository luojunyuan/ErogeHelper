using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        private const int WsExToolWindow = 0x00000080;

        public static void HideWindowInAltTab(Window window)
        {
            var windowInterop = new WindowInteropHelper(window);
            var exStyle = User32.GetWindowLong(new HWND(windowInterop.Handle), 
                                                   User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= WsExToolWindow;
            _ = User32.SetWindowLong(new HWND(windowInterop.Handle), User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }
    }
}
