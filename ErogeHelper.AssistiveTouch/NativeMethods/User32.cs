using System.Runtime.InteropServices;

namespace ErogeHelper.AssistiveTouch.NativeMethods
{
    internal partial class User32
    {
        private const string User32Dll = "user32.dll";

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        public static extern HWINEVENTHOOK SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc pfnWinEventProc, uint idProcess, uint idThread, WINEVENT dwFlags);

        [DllImport(User32Dll, SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport(User32Dll, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(User32Dll, ExactSpelling = true, SetLastError = true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref System.Drawing.Point lpPoints, uint cPoints = 1);

        [DllImport(User32Dll)]
        public static extern bool UnhookWinEvent(HWINEVENTHOOK hWinEventHook);

        // Above GameWindowHooker

        [DllImport(User32Dll, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, WindowLongFlags nIndex);

        public static int SetWindowLong(IntPtr hWnd, WindowLongFlags nIndex, int dwNewLong)
        {
            IntPtr ret;
            if (IntPtr.Size == 4)
                ret = (IntPtr)SetWindowLongPtr32(hWnd, nIndex, (IntPtr)dwNewLong);
            else
                ret = SetWindowLongPtr64(hWnd, nIndex, (IntPtr)dwNewLong);
            if (ret == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            return ret.ToInt32();
        }

        [DllImport(User32Dll, SetLastError = true, EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLongPtr32(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);

        [DllImport(User32Dll, SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        // Above for HwndTools

        // Below for touch to mouse hook

        [DllImport(User32Dll, SetLastError = true, ExactSpelling = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport(User32Dll, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hmod, int dwThreadId);

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(User32Dll)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
        public static extern void mouse_event(MOUSEEVENTF dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport(User32Dll, SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        // For fullscreen

        [DllImport(User32Dll, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(User32Dll)]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        // MainWindow initialize

        [DllImport(User32Dll, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        // Menu function
        [DllImport(User32Dll, SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, [Optional] int lParam);

    }
}
