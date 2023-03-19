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


        [DllImport(Lib.User32, SetLastError = false, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);



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

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);



        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void WinEventProc(HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);


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
    }
}
