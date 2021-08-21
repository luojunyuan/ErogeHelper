using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        private static readonly Version OsVersion = Environment.OSVersion.Version;

        public static bool IsOSWindows8OrNewer { get; } = OsVersion >= new Version(6, 2);

        public static void HideWindowInAltTab(Window window) =>
            HideWindowInAltTab(new WindowInteropHelper(window).Handle);

        public static void HideWindowInAltTab(IntPtr windowHandle)
        {
            const int WsExToolWindow = 0x00000080;

            var exStyle = User32.GetWindowLong(new HWND(windowHandle),
                                               User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= WsExToolWindow;
            _ = User32.SetWindowLong(new HWND(windowHandle), User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }

        public static string GetOSInfo()
        {
            Version Windows7 = new(6, 1);
            Version Windows8 = new(6, 2);
            Version Windows81 = new(6, 3);
            Version Windows10 = new(10, 0);

            var osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            if (OsVersion >= Windows10)
            {
                var releaseId = Environment.OSVersion.Version.Build switch
                {
                    20348 => "21H2",
                    19043 => "21H1",
                    19042 => "20H2",
                    19041 => "2004",
                    18363 => "1909",
                    18362 => "1903",
                    17763 => "1809",
                    17134 => "1803",
                    16299 => "1709",
                    15063 => "1703",
                    14393 => "1607",
                    10586 => "1511",
                    10240 => "1507",
                    _ => $"{Environment.OSVersion.Version.Build}",
                };

                return $"Windows 10 {releaseId} {osBit}";
            }
            else if (OsVersion >= Windows81)
            {
                return $"Windows 8.1 {osBit}";
            }
            else if (OsVersion >= Windows8)
            {
                return $"Windows 8 {osBit}";
            }
            else if (OsVersion >= Windows7)
            {
                return $"Windows 7 {Environment.OSVersion.ServicePack} {osBit}";
            }
            else
            {
                return Environment.OSVersion.VersionString;
            }
        }

        public static string Md5Calculate(byte[] buffer, bool toUpper = false)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(buffer);

            var sb = new StringBuilder();

            var format = toUpper ? "X2" : "x2";

            foreach (var byteItem in hash)
            {
                sb.Append(byteItem.ToString(format));
            }

            return sb.ToString();
        }

        public static string Md5Calculate(string str, Encoding encoding, bool toUpper = false)
        {
            var buffer = encoding.GetBytes(str);
            return Md5Calculate(buffer, toUpper);
        }

        public static string Md5Calculate(string str, bool toUpper = false) =>
            Md5Calculate(str, Encoding.Default, toUpper);
    }
}
