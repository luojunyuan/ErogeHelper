using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace ErogeHelper.VirtualKeyboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IntPtr GameWindowHandle { get; set; } = (IntPtr)int.Parse(Environment.GetCommandLineArgs()[1]);

        private void Application_Startup(object sender, StartupEventArgs e) => DisableWPFTabletSupport();

        private static void DisableWPFTabletSupport()
        {
            TabletDeviceCollection devices = Tablet.TabletDevices;

            if (devices.Count > 0)
            {
                object stylusLogic = typeof(InputManager).InvokeMember("StylusLogic",
                            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, InputManager.Current, null);
                if (stylusLogic != null)
                {
                    Type stylusLogicType = stylusLogic.GetType();
                    while (devices.Count > 0)
                    {
                        stylusLogicType.InvokeMember("OnTabletRemoved",
                                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                                null, stylusLogic, new object[] { (uint)0 });
                    }
                }

            }
        }
    }

    public static class User32
    {
        private const string user32 = "user32.dll";

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, WindowLongFlags nIndex);

        public static int SetWindowLong(IntPtr hWnd, WindowLongFlags nIndex, int dwNewLong)
        {
            IntPtr ret;
            if (IntPtr.Size == 4)
                ret = SetWindowLongPtr32(hWnd, nIndex, (IntPtr)dwNewLong);
            else
                ret = SetWindowLongPtr64(hWnd, nIndex, (IntPtr)dwNewLong);
            if (ret == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            return ret.ToInt32();
        }

        [DllImport(user32, SetLastError = true, EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);

        [DllImport(user32, SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);

        public enum WindowLongFlags
        {
            GWL_EXSTYLE = -20,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

        [Flags]
        public enum WindowStylesEx : uint
        {
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_NOACTIVATE = 0x08000000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_WINDOWEDGE = 0x00000100,
        }

        [DllImport(user32, SetLastError = false, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(user32, SetLastError = false, ExactSpelling = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc pfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void WinEventProc(HWINEVENTHOOK hWinEventHook, uint winEvent, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct HWINEVENTHOOK
        {
            private readonly IntPtr handle;
            public HWINEVENTHOOK(IntPtr preexistingHandle) => handle = preexistingHandle;

            public static HWINEVENTHOOK NULL => new HWINEVENTHOOK(IntPtr.Zero);

            public bool IsNull => handle == IntPtr.Zero;

            public static explicit operator IntPtr(HWINEVENTHOOK h) => h.handle;

            public static implicit operator HWINEVENTHOOK(IntPtr h) => new HWINEVENTHOOK(h);

            public static bool operator !=(HWINEVENTHOOK h1, HWINEVENTHOOK h2) => !(h1 == h2);

            public static bool operator ==(HWINEVENTHOOK h1, HWINEVENTHOOK h2) => h1.Equals(h2);

            public override bool Equals(object obj) => obj is HWINEVENTHOOK h && handle == h.handle;

            public override int GetHashCode() => handle.GetHashCode();

            public IntPtr DangerousGetHandle() => handle;
        }

        [DllImport(user32, SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out System.Drawing.Rectangle lpRect);

        [DllImport(user32, SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hwnd, out System.Drawing.Rectangle lpRect);

        [DllImport(user32)]
        public static extern bool UnhookWinEvent(HWINEVENTHOOK hWinEventHook);
    }
}
