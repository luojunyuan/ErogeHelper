using System.Diagnostics;
using System.Security.Principal;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace ErogeHelper.Platform.MISC;

internal class ToastManagement : IToastManagement
{
    // Tip: CustomNotification
    // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/CustomNotificatios.md
    private readonly Notifier DesktopNotifier = new(cfg =>
    {
        cfg.PositionProvider =
            new PrimaryScreenPositionProvider(
                corner: Corner.BottomRight,
                offsetX: 16,
                offsetY: 12);

        cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
            notificationLifetime: TimeSpan.FromMilliseconds(IToastManagement.ToastDurationTime),
            maximumNotificationCount: MaximumNotificationCount.UnlimitedNotifications());

        cfg.DisplayOptions.TopMost = true;
    });

    public void Show(string mainText) =>
        DesktopNotifier.ShowInformation(
            mainText,
            new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false });

    public Task ShowAsync(string mainText, Stopwatch toastLifetimeTimer)
    {
        DesktopNotifier.ShowInformation(
            mainText,
            new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false });

        return Task.CompletedTask;
    }

    public void InAdminModeToastTip()
    {
        var current = WindowsIdentity.GetCurrent();
        var windowsPrincipal = new WindowsPrincipal(current);
        if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            return;

        Show("ErogeHelper is running in Admin");
    }

    public void ClearToast() { }
}
