using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace ErogeHelper.Function.WpfExtend;

internal static class WpfHelper
{
    public static void ShowWindow<T>() where T : Window, new()
    {
        var win = Application.Current.Windows
            .Cast<Window>()
            .Where(w => w is T)
            .FirstOrDefault();

        if (win is not null)
        {
            win.Activate();
            if (win.WindowState == WindowState.Minimized)
                win.WindowState = WindowState.Normal;
        }
        else
        {
            win = new T();
            win.Show();
            win.Activate();
        }
    }

    public static nint GetWpfWindowHandle(Window window) => new WindowInteropHelper(window).EnsureHandle();

    /// <summary>
    /// Not immediately
    /// </summary>
    /// <param name="gameHandle">game real window handle</param>
    /// <returns>true if game is in fullscreen status</returns>
    public static bool IsGameForegroundFullscreen(nint gameHandle)
    {
        if (gameHandle == nint.Zero)
            return false;

        User32.GetWindowRect(gameHandle, out var rect);
        var fullScreenGameRect = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        foreach (var screen in WpfScreenHelper.Screen.AllScreens)
        {
            if (fullScreenGameRect == screen.Bounds)
                return true;
        }
        return false;
    }
}
