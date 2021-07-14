using ErogeHelper.Common.Contract;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
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
        
        // Tip: CustomNotification with enforce get over process
        // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/CustomNotificatios.md
        public static Notifier DesktopNotifier => new(cfg =>
            {
                cfg.PositionProvider =
                    new PrimaryScreenPositionProvider(
                        corner: Corner.BottomRight,
                        offsetX: 16,
                        offsetY: 12);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromMilliseconds(ConstantValues.ToastDuration),
                    maximumNotificationCount: MaximumNotificationCount.UnlimitedNotifications());

                cfg.DisplayOptions.TopMost = true;
            });

        private static readonly Version _osVersion = Environment.OSVersion.Version;

        public static bool IsOSWindows8OorNewer => _osVersion >= new Version(6, 2);

        public static string GetOSInfo()
        {
            Version Windows7 = new(6, 1);
            Version Windows8 = new(6, 2);
            Version Windows81 = new(6, 3);
            Version Windows10 = new(10, 0);

            var osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            
            if (_osVersion >= Windows10)
            {
                var releaseId = Environment.OSVersion.Version.Build switch
                {
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
                    _ => Environment.OSVersion.Version.Build.ToString(),
                };

                return $"Windows 10 {releaseId} {osBit}";
            }
            else if (_osVersion >= Windows81)
            {
                return $"Windows 8.1 {osBit}";
            }
            else if (_osVersion >= Windows8)
            { 
                return $"Windows 8 {osBit}";
            }
            else if (_osVersion >= Windows7)
            {
                return $"Windows 7 {Environment.OSVersion.ServicePack} {osBit}";
            }
            else
            {
                return Environment.OSVersion.VersionString;
            }
        }
    }
}
