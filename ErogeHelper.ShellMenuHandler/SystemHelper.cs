using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ErogeHelper.ShellMenuHandler
{
    static class SystemHelper
    {
        public static bool Is4KDisplay()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            // 10 = VERTRES
            // 90 = LOGPIXELSY
            // 117 = DESKTOPVERTRES
            int logicDpi = GetDeviceCaps(desktop, 10);
            int logY = GetDeviceCaps(desktop, 90);
            int realDpi = GetDeviceCaps(desktop, 117);

            g.ReleaseHdc();

            return (realDpi / logicDpi == 2) || (logY / 96 == 2);
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
    }
}
