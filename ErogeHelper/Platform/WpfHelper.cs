using System.Windows;
using System.Windows.Interop;
using ErogeHelper.Shared.Contracts;
using Microsoft.Win32;
using Vanara.PInvoke;

namespace ErogeHelper.Platform;

internal static class WpfHelper
{
    public static HWND GetWpfWindowHandle(Window window) => new(new WindowInteropHelper(window).EnsureHandle());

    public static bool IsDpiCompatibilitySet(string exeFilePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(ConstantValue.ApplicationCompatibilityRegistryPath, true)
            ?? Registry.CurrentUser.CreateSubKey(ConstantValue.ApplicationCompatibilityRegistryPath);

        var currentValue = key.GetValue(exeFilePath) as string;
        if (string.IsNullOrEmpty(currentValue))
            return false;

        var DpiSettings = new List<string>() { "HIGHDPIAWARE", "DPIUNAWARE", "GDIDPISCALING DPIUNAWARE" };
        var currentValueList = currentValue.Split(' ').ToList();
        return DpiSettings.Any(v => currentValueList.Contains(v));
    }

    public static void SetDPICompatibilityAsApplication(string exeFilePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(ConstantValue.ApplicationCompatibilityRegistryPath, true)
            ?? Registry.CurrentUser.CreateSubKey(ConstantValue.ApplicationCompatibilityRegistryPath);

        var currentValue = key.GetValue(exeFilePath) as string;
        if (string.IsNullOrEmpty(currentValue))
            key.SetValue(exeFilePath, "~ HIGHDPIAWARE");
        else
        {
            key.SetValue(exeFilePath, currentValue + " HIGHDPIAWARE");
        }
    }

    /// <summary>
    /// Not immediately
    /// </summary>
    /// <param name="gameHwnd">game real window handle</param>
    /// <returns>true if game is in fullscreen status</returns>
    public static bool IsGameForegroundFullscreen(HWND gameHwnd)
    {
        User32.GetWindowRect(gameHwnd, out var rect);
        var fullScreenGameRect = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        foreach (var screen in WpfScreenHelper.Screen.AllScreens)
        {
            if (fullScreenGameRect.Contains(screen.PixelBounds))
                return true;
        }
        return false;
    }
}
