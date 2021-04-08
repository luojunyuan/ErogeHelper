using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ErogeHelper.ShellMenuHandler
{
    internal static class SystemHelper
    {
        public static bool Is4KDisplay()
        {
            var g = Graphics.FromHwnd(IntPtr.Zero);
            var desktop = g.GetHdc();

            // 10 = VERTRES
            // 90 = LOGPIXELSY
            // 117 = DESKTOPVERTRES
            var logicDpi = GetDeviceCaps(desktop, 10);
            var logY = GetDeviceCaps(desktop, 90);
            var realDpi = GetDeviceCaps(desktop, 117);

            g.ReleaseHdc();

            return (realDpi / logicDpi == 2) || (logY / 96 == 2);
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hDc, int nIndex);
    }
}
