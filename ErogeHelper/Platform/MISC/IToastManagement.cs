using System.Diagnostics;

namespace ErogeHelper.Platform.MISC;

public interface IToastManagement
{
    const int ToastDurationTime = 5000;

    void Show(string mainText);

    Task ShowAsync(string mainText, Stopwatch toastLifetimeTimer);

    void InAdminModeToastTip();

    void ClearToast();
}
