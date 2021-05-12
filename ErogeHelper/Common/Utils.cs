using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Windows.Sdk;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public static void HideWindowInAltTab(Window window)
        {
            var windowInterop = new WindowInteropHelper(window);
            var exStyle = PInvoke.GetWindowLong(new HWND(windowInterop.Handle), GetWindowLongPtr_nIndex.GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW;
            _ = PInvoke.SetWindowLong(new HWND(windowInterop.Handle), GetWindowLongPtr_nIndex.GWL_EXSTYLE, exStyle);
        }
    }
}
