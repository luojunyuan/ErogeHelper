using Microsoft.Windows.Sdk;
using System.Windows;
using System.Windows.Interop;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        private const int WsExToolWindow = 0x00000080;

        public static void HideWindowInAltTab(Window window)
        {
            var windowInterop = new WindowInteropHelper(window);
            var exStyle = PInvoke.GetWindowLong(new HWND(windowInterop.Handle), GetWindowLongPtr_nIndex.GWL_EXSTYLE);
            exStyle |= WsExToolWindow;
            _ = PInvoke.SetWindowLong(new HWND(windowInterop.Handle), GetWindowLongPtr_nIndex.GWL_EXSTYLE, exStyle);
        }

        public static unsafe int GetProcessIdByHandle(HWND handle)
        {
            uint lpdwProcessId = 0;
            _ = PInvoke.GetWindowThreadProcessId(handle, &lpdwProcessId);
            return (int)lpdwProcessId;
        }
    }
}
