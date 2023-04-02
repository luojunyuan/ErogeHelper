using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Preference
{
    internal class UACShield
    {
        public static BitmapSource Image { get; }

        static UACShield()
        {
            var iconInfo = new SHSTOCKICONINFO();
            iconInfo.cbSize = (uint)Marshal.SizeOf(iconInfo);
            SHGetStockIconInfo(SHSTOCKICONID.SIID_SHIELD, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON, ref iconInfo);

            Image = Imaging.CreateBitmapSourceFromHIcon(iconInfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        [DllImport("shell32.dll", SetLastError = false)]
        public static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        public enum SHSTOCKICONID : uint
        {
            SIID_SHIELD = 77
        }

        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICON = 0x000000100,
            SHGSI_SMALLICON = 0x000000001
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }
    }
}
