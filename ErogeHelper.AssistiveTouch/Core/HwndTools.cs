using ErogeHelper.AssistiveTouch.NativeMethods;

namespace ErogeHelper.AssistiveTouch.Helper;

public static class HwndTools
{
    public static void RemovePopupAddChildStyle(IntPtr handle)
    {
        var style = (uint)User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE);
        style = style & ~(uint)User32.WindowStyles.WS_POPUP | (uint)User32.WindowStyles.WS_CHILD;
        User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE, (int)style);
    }
}
