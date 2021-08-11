using CommunityToolkit.WinUI.Notifications;
using ErogeHelper.Common.Contracts;
using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Vanara.PInvoke;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        private static readonly Version OsVersion = Environment.OSVersion.Version;

        // Tip: CustomNotification
        // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/CustomNotificatios.md
        public static readonly Notifier DesktopNotifier = new(cfg =>
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
            byte[] hash = md5.ComputeHash(buffer);

            var sb = new StringBuilder();

            string format = toUpper ? "X2" : "x2";

            foreach (byte byteItem in hash)
            {
                sb.Append(byteItem.ToString(format));
            }

            return sb.ToString();
        }

        public static string Md5Calculate(string str, Encoding encoding, bool toUpper = false)
        {
            byte[] buffer = encoding.GetBytes(str);
            return Md5Calculate(buffer, toUpper);
        }

        public static string Md5Calculate(string str, bool toUpper = false) =>
            Md5Calculate(str, Encoding.Default, toUpper);

        public static void AdministratorToast()
        {
            var current = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(current);
            if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                return;

            if (IsOSWindows8OrNewer)
            {
                new ToastContentBuilder()
                    .AddText("ErogeHelper is running in Admin")
                    .Show(toast =>
                    {
                        toast.Group = "eh";
                        toast.Tag = "eh";
                        // ExpirationTime bugged with InvalidCastException in .Net5
                        // ExpirationTime can not work and bugged with using
                        // ToastNotificationManagerCompat.History.Clear() in .Net6
                        //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
                    });

                Thread.Sleep(ConstantValues.ToastDuration);
                ToastNotificationManagerCompat.History.Clear();
            }
            else
            {
                DesktopNotifier.ShowInformation(
                    "ErogeHelper is running in Admin",
                    new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false });
            }
        }
    }
}
