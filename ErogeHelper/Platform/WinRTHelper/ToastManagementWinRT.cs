using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using CommunityToolkit.WinUI.Notifications;
using ErogeHelper.Platform.MISC;
using Splat;

namespace ErogeHelper.Platform.WinRTHelper;

internal class ToastManagementWinRT : IToastManagement, IEnableLogger
{
    public ToastManagementWinRT() =>
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            if (toastArgs.Argument.Length == 0)
            {
                this.Log().Debug("Toast Clicked");
                return;
            }
            var toastArguments = ToastArguments.Parse(toastArgs.Argument);
            this.Log().Debug(toastArguments.ToString());
        };

    public void Show(string mainText)
    {
        new ToastContentBuilder()
            .AddText(mainText)
            .Show(toast =>
            {
                toast.Group = "eh";
                toast.Tag = "eh";
                // ExpirationTime bugged with InvalidCastException in .Net5
                // ExpirationTime can not work and bugged with using
                // ToastNotificationManagerCompat.History.Clear() in .Net6
                //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            });

        Thread.Sleep(IToastManagement.ToastDurationTime);
        ToastNotificationManagerCompat.History.Clear();
    }

    public async Task ShowAsync(string mainText, Stopwatch toastLifetimeTimer)
    {
        new ToastContentBuilder()
            .AddText(mainText)
            .Show(toast =>
            {
                toast.Group = "eh";
                toast.Tag = "eh";
                // ExpirationTime bugged with InvalidCastException in .Net5
                // ExpirationTime can not work and bugged with using
                // ToastNotificationManagerCompat.History.Clear() in .Net6
                //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            });

        toastLifetimeTimer.Restart();
        await Task.Delay(IToastManagement.ToastDurationTime).ConfigureAwait(false);
        if (toastLifetimeTimer.ElapsedMilliseconds >= IToastManagement.ToastDurationTime)
        {
            ToastNotificationManagerCompat.History.Clear();
            toastLifetimeTimer.Stop();
        }
    }

    public void InAdminModeToastTip()
    {
        var current = WindowsIdentity.GetCurrent();
        var windowsPrincipal = new WindowsPrincipal(current);
        if (!windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            return;

        Show("ErogeHelper is running in Admin");
    }

    public void ClearToast()
    {
        try
        {
            ToastNotificationManagerCompat.History.Clear();
        }
        catch (COMException ex)
        {
            // When run on early system like 1507 1511 for the first time would throw error #16
            this.Log().Debug(ex.Message);
        }
    }
}
