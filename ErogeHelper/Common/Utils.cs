using Microsoft.Win32;
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

        public static readonly string MachineGUID = GetMachineGUID();

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

            var osBit = Environment.Is64BitOperatingSystem ? "64" : "32";

            if (OsVersion >= Windows10)
            {
                var releaseId = Environment.OSVersion.Version.Build switch
                {
                    22000 => "21H2 arm",
                    20348 => "21H2 x86_",
                    19043 => "21H1 x86_",
                    19042 => "20H2 x86_",
                    19041 => "2004 x86_", // Current target WinRT-SDK version
                    18363 => "1909 x86_",
                    18362 => "1903 x86_",
                    17763 => "1809 x86_",
                    17134 => "1803 x86_",
                    16299 => "1709 x86_",
                    15063 => "1703 x86_",
                    14393 => "1607 x86_",
                    10586 => "1511 x86_",
                    10240 => "1507 x86_",
                    _ => $"{Environment.OSVersion.Version.Build}",
                };

                return $"Windows 10 {releaseId}{osBit}";
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

        private static string GetMachineGUID()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                var keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var keyX64 = keyBaseX64.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX64 = keyX64?.GetValue("MachineGuid", string.Empty);

                if (resultObjX64 is not null)
                {
                    return resultObjX64.ToString() ?? string.Empty;
                }
            }
            else
            {
                var keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var keyX86 = keyBaseX86.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX86 = keyX86?.GetValue("MachineGuid", string.Empty);

                if (resultObjX86 != null)
                {
                    return resultObjX86.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
