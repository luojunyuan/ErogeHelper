using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel;

// Directly reference by view so put this in viewmodel
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

    internal static void WindowLostFocus(HWND windowHandle, bool lostFocus)
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

    internal static void WindowBlur(HWND windowHandle, bool enable)
    {
        var accent = new AccentPolicy
        {
            AccentState = enable ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_DISABLED
        };

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        _ = SetWindowCompositionAttribute(windowHandle.DangerousGetHandle(), ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    // DwmEnableBlurBehindWindow in Win7

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        ACCENT_DISABLED,
        ACCENT_ENABLE_GRADIENT,
        ACCENT_ENABLE_TRANSPARENTGRADIENT,
        ACCENT_ENABLE_BLURBEHIND,
        ACCENT_INVALID_STATE,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19,
        // ...
    }
}
