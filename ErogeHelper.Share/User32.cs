using System.Runtime.InteropServices;

namespace ErogeHelper.Share
{
    public static class Lib
    {
        public const string User32 = "user32.dll";
        public const string Kernel32 = "kernel32.dll";
    }

    public partial class User32
    {
        [DllImport(Lib.User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowLong(HWND hWnd, WindowLongFlags nIndex);

        public static int SetWindowLong(HWND hWnd, WindowLongFlags nIndex, int dwNewLong)
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

        [DllImport(Lib.User32, SetLastError = true, EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLongPtr32(HWND hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);


        [DllImport(Lib.User32, SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HWND hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);


        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(HWND hWnd);

        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(HWND hWnd, ShowWindowCommand nCmdShow);

        [DllImport(Lib.User32, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Security.SecurityCritical]
        public static extern bool GetClientRect(HWND hWnd, out RECT lpRect);


        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);

        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(HWND hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowsProc([In] HWND hwnd, [In] IntPtr lParam);

        [DllImport(Lib.User32, SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport(Lib.User32, SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MoveWindow(HWND hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        [DllImport(Lib.User32, SetLastError = true, ExactSpelling = true)]
        public static extern HWND SetParent(HWND hWndChild, HWND hWndNewParent);

        [DllImport(Lib.User32, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Security.SecurityCritical]
        public static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

        [DllImport(Lib.User32, ExactSpelling = true, SetLastError = true)]
        public static extern int MapWindowPoints(HWND hWndFrom, HWND hWndTo, ref RECT lpPoints, uint cPoints = 2);
        [DllImport(Lib.User32, ExactSpelling = true, SetLastError = true)]
        public static extern int MapWindowPoints(HWND hWndFrom, HWND hWndTo, ref POINT lpPoints, uint cPoints = 1);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);



        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void WinEventProc(HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        public static extern HWINEVENTHOOK SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc pfnWinEventProc, uint idProcess, uint idThread, WINEVENT dwFlags);

        [DllImport(Lib.User32)]
        public static extern bool UnhookWinEvent(HWINEVENTHOOK hWinEventHook);


        [DllImport(Lib.User32)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, [Optional] int lParam);

        [DllImport(Lib.User32, SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        // For touch to mouse hook
        [DllImport(Lib.User32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeHHOOK SetWindowsHookEx(HookType idHook, HookProc lpfn, [In, Optional] IntPtr hmod, [Optional] int dwThreadId);
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        public static extern IntPtr CallNextHookEx(HHOOK hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        public static extern void mouse_event(MOUSEEVENTF dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport(Lib.User32, SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);
    }
}
