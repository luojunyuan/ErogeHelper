using Vanara.PInvoke;

namespace ErogeHelper.Model.Services
{
    public static class HwndTools
    {
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
    }
}
