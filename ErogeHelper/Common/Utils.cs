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
        public static readonly Notifier DesktopNotifier = new(cfg =>
            {
                cfg.PositionProvider =
                    new PrimaryScreenPositionProvider(
                        corner: Corner.BottomRight,
                        offsetX: 16,
                        offsetY: 12);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromMilliseconds(ConstantValues.ToastLifetime),
                    maximumNotificationCount: MaximumNotificationCount.UnlimitedNotifications());

                cfg.DisplayOptions.TopMost = true;
            });
        // Usage
        // Note: Would throw Exceptions if enforce getting over process
        // System.Threading.Tasks.TaskCanceledException(In System.Private.CoreLib.dll)
        // System.TimeoutException(In WindowsBase.dll)
        //Utils.DesktopNotifier.ShowInformation(
        //    "ErogeHelper is already running!",
        //    new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false });
    }
}
