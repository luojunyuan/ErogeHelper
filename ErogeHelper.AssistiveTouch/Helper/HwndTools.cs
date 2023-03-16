using System.Diagnostics;
using System.Runtime.InteropServices;
using ErogeHelper.Share;

namespace ErogeHelper.AssistiveTouch.Helper;

public static class HwndTools
{
    public static void HideWindowInAltTab(nint windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            return;

        const int wsExToolWindow = 0x00000080;

        var exStyle = User32.GetWindowLong(windowHandle,
            User32.WindowLongFlags.GWL_EXSTYLE);
        exStyle |= wsExToolWindow;
        _ = User32.SetWindowLong(windowHandle, User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
    }

    public static void WindowLostFocus(nint windowHandle, bool loseFocus)
    {
        if (windowHandle == IntPtr.Zero)
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

    public static void RemovePopupAddChildStyle(IntPtr handle)
    {
        var style = (uint)User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE);
        style = style & ~(uint)User32.WindowStyles.WS_POPUP | (uint)User32.WindowStyles.WS_CHILD;
        User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE, (int)style);
    }

    /// <param name="activeGame">Bring game to front</param>
    public static IntPtr FindMainWindowHandle(Process proc, bool activeGame = true)
    {
        const int WaitGameStartTimeout = 20000;
        const int UIMinimumResponseTime = 50;

        proc.WaitForInputIdle(WaitGameStartTimeout);
        proc.Refresh();
        // Might be zero at first
        var gameHwnd = proc.MainWindowHandle;

        if (activeGame && User32.IsIconic(proc.MainWindowHandle))
        {
            User32.ShowWindow(proc.MainWindowHandle, ShowWindowCommand.SW_RESTORE);
        }

        User32.GetClientRect(gameHwnd, out var clientRect);

        if (clientRect.bottom > GoodWindowHeight &&
            clientRect.right > GoodWindowWidth)
        {
            return gameHwnd;
        }
        else
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            while (spendTime.Elapsed.TotalMilliseconds < WaitGameStartTimeout)
            {
                if (proc.HasExited)
                    return IntPtr.Zero;

                // Process.MainGameHandle should included in handles
                var handles = GetRootWindowsOfProcess(proc.Id);
                foreach (var handle in handles)
                {
                    User32.GetClientRect(handle, out clientRect);
                    if (clientRect.bottom > GoodWindowHeight &&
                        clientRect.right > GoodWindowWidth)
                    {
                        return handle.DangerousGetHandle();
                    }
                }
                Thread.Sleep(UIMinimumResponseTime);
            }
            throw new ArgumentException("Find window handle failed");
        }
    }

    // private const int VNRWindowWidth = 160;
    // private const int VNRWindowHeight = 120;
    // private const int MinWindowSize = 12;
    private const int GoodWindowWidth = 500;
    private const int GoodWindowHeight = 320;

    private static IEnumerable<HWND> GetRootWindowsOfProcess(int pid)
    {
        var rootWindows = GetChildWindows(IntPtr.Zero);
        var dsProcRootWindows = new List<HWND>();
        foreach (var hWnd in rootWindows)
        {
            _ = User32.GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
            if (lpdwProcessId == pid)
                dsProcRootWindows.Add(hWnd);
        }
        return dsProcRootWindows;
    }

    private static IEnumerable<HWND> GetChildWindows(HWND parent)
    {
        List<HWND> result = new();
        var listHandle = GCHandle.Alloc(result);
        try
        {
            static bool ChildProc(HWND handle, IntPtr pointer)
            {
                var gch = GCHandle.FromIntPtr(pointer);
                if (gch.Target is not List<HWND> list)
                {
                    throw new InvalidCastException("GCHandle Target could not be cast as List<HWND>");
                }
                list.Add(handle);
                return true;
            }
            User32.EnumChildWindows(parent, ChildProc, GCHandle.ToIntPtr(listHandle));
        }
        finally
        {
            if (listHandle.IsAllocated)
                listHandle.Free();
        }
        return result;
    }
}
