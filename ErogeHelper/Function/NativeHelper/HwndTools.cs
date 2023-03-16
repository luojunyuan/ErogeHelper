using Vanara.PInvoke;

namespace ErogeHelper.Function.NativeHelper;

public static class HwndTools
{
    public static void HideWindowInAltTab(nint windowHandle)
    {
        if (windowHandle == nint.Zero)
            return;

        const int wsExToolWindow = 0x00000080;

        var exStyle = User32.GetWindowLong(windowHandle,
            User32.WindowLongFlags.GWL_EXSTYLE);
        exStyle |= wsExToolWindow;
        _ = User32.SetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
    }

    public static void WindowLostFocus(nint windowHandle, bool loseFocus)
    {
        if (windowHandle == nint.Zero)
            return;

        var exStyle = User32.GetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE);
        if (loseFocus)
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
}
