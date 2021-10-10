using ErogeHelper.Common.Contracts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace ErogeHelper
{
    public static class Utils
    {
        private static readonly Version OsVersion = new(Environment.OSVersion.Version.Major,
                                                        Environment.OSVersion.Version.Minor,
                                                        Environment.OSVersion.Version.Build);

        public static readonly string MachineGuid = GetMachineGuid();

        public static bool IsOsWindows8OrNewer { get; } = OsVersion >= new Version(6, 2);

        public static HWND GetWpfWindowHandle(Window window) => new(new WindowInteropHelper(window).EnsureHandle());

        public static void SetDPICompatibilityAsApplication(string exeFilePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(ConstantValues.ApplicationCompatibilityRegistryPath, true)
                ?? Registry.CurrentUser.CreateSubKey(ConstantValues.ApplicationCompatibilityRegistryPath);

            var currentValue = key.GetValue(exeFilePath) as string;
            if (string.IsNullOrEmpty(currentValue))
                key.SetValue(exeFilePath, "~ HIGHDPIAWARE");
            else
            {
                key.SetValue(exeFilePath, currentValue + " HIGHDPIAWARE");
            }
        }

        public static bool AlreadyHasDpiCompatibilitySetting(string exeFilePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(ConstantValues.ApplicationCompatibilityRegistryPath, true)
                ?? Registry.CurrentUser.CreateSubKey(ConstantValues.ApplicationCompatibilityRegistryPath);

            var currentValue = key.GetValue(exeFilePath) as string;
            if (string.IsNullOrEmpty(currentValue))
                return false;

            var DpiSettings = new List<string>() { "HIGHDPIAWARE", "DPIUNAWARE", "GDIDPISCALING DPIUNAWARE" };
            var currentValueList = currentValue.Split(' ').ToList();
            return DpiSettings.Any(v => currentValueList.Contains(v));
        }

        /// <summary>
        /// Not so immediately
        /// </summary>
        /// <param name="gameHwnd">game real window handle</param>
        /// <returns>true if game is in fullscreen status</returns>
        public static bool IsGameForegroundFullscreen(HWND gameHwnd)
        {
            User32.GetWindowRect(gameHwnd, out var rect);
            foreach (var screen in WpfScreenHelper.Screen.AllScreens)
            {
                var fullScreenGameRect = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                if (fullScreenGameRect.Contains(screen.PixelBounds))
                    return true;
            }
            return false;
        }

        public static List<Process> GetProcessesByfriendlyName(string friendlyName)
        {
            var processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName(friendlyName));
            processes.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
            if (!friendlyName.Equals("main.bin", StringComparison.Ordinal))
                processes.AddRange(Process.GetProcessesByName("main.bin"));
            return processes;
        }

        public static void HideWindowInAltTab(Window window) =>
            HideWindowInAltTab(new WindowInteropHelper(window).Handle);

        public static void HideWindowInAltTab(HWND windowHandle)
        {
            const int wsExToolWindow = 0x00000080;

            var exStyle = User32.GetWindowLong(windowHandle,
                                               User32.WindowLongFlags.GWL_EXSTYLE);
            exStyle |= wsExToolWindow;
            _ = User32.SetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
        }

        public static void WindowLostFocus(HWND windowHandle, bool lostFocus)
        {
            var exStyle = User32.GetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE);
            if (lostFocus)
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

        public static string GetOsInfo()
        {
            var windows7 = new Version(6, 1);
            var windows8 = new Version(6, 2);
            var windows81 = new Version(6, 3);
            var windows10 = new Version(10, 0);
            var windows11 = new Version(10, 0, 22000);

            var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            var buildVersion = Environment.OSVersion.Version.Build;
            var releaseId = buildVersion switch
            {
                22471 => "Insider Preview",
                22454 => "Insider Preview",
                22000 => "21H2",
                19044 => "21H2",
                19043 => "21H1",
                19042 => "20H2",
                19041 => "2004", // Current target WinRT-SDK version
                18363 => "1909",
                18362 => "1903",
                17763 => "1809",
                17134 => "1803",
                16299 => "1709",
                15063 => "1703",
                14393 => "1607",
                10586 => "1511",
                10240 => "1507",
                _ => buildVersion.ToString()
            };

            // Not reliable
            // string osName = Registry.GetValue(ConstantValues.HKLMWinNTCurrent, "productName", "")?.ToString() ?? string.Empty;

            var windowVersionString =
                OsVersion >= windows11 ? $"Windows 11 {releaseId}" :
                OsVersion >= windows10 ? $"Windows 10 {releaseId}" :
                OsVersion >= windows81 ? "Windows 8.1" :
                OsVersion >= windows8 ? "Windows 8" :
                OsVersion >= windows7 ? $"Windows 7 {Environment.OSVersion.ServicePack}" :
                Environment.OSVersion.VersionString;

            return $"{windowVersionString} {architecture}";
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

        private static string GetMachineGuid()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                var keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var keyX64 = keyBaseX64.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX64 = keyX64?.GetValue("MachineGuid", string.Empty);

                if (resultObjX64 is not null)
                    return resultObjX64.ToString() ?? string.Empty;
            }
            else
            {
                var keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var keyX86 = keyBaseX86.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX86 = keyX86?.GetValue("MachineGuid", string.Empty);

                if (resultObjX86 != null)
                    return resultObjX86.ToString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
