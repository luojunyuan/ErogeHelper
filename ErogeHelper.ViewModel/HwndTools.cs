using System.Runtime.InteropServices;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel;

// Directly reference by view so put this in viewmodel
public static class HwndTools
{
    public static IntPtr GetRealGameHandle() =>
        DependencyResolver.GetService<IGameDataService>().GameRealWindowHandle.DangerousGetHandle();

    public static Func<HWND, bool> IsGameFullscreenCallback { get; set; } = null!;

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
        if (windowHandle.IsNull)
        {
            return;
        }

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

    // Alternative implement? https://www.cnblogs.com/lan-mei/archive/2012/05/11/2495740.html
    // Do not make it complex just curious the difference
    // SetWindowCompositionAttribute is win10 only, for Win7 DwmEnableBlurBehindWindow 

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
        WCA_ACCENT_POLICY = 19,
    }
}
