using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming XXX: allowed uppercase naming
// ReSharper disable IdentifierTypo XXX: allowed all (uppercase) naming

namespace ErogeHelper.Common
{
    public static class NativeMethods
    {
        public delegate void WinEventDelegate(IntPtr hWinEventHook,
                                              SWEHEvents eventType,
                                              IntPtr hwnd,
                                              SWEH_ObjectId idObject,
                                              long idChild,
                                              uint dwEventThread,
                                              uint dwmsEventTime);

        public static IntPtr WinEventHookRange(SWEHEvents eventFrom,
                                               SWEHEvents eventTo,
                                               WinEventDelegate @delegate,
                                               uint idProcess, uint idThread)
        {
            return UnsafeNativeMethods.SetWinEventHook(eventFrom, eventTo,
                                                       IntPtr.Zero, @delegate,
                                                       idProcess, idThread,
                                                       WinEventHookInternalFlags);
        }

        public static IntPtr WinEventHookOne(SWEHEvents @event, WinEventDelegate @delegate, uint idProcess, uint idThread)
        {
            return UnsafeNativeMethods.SetWinEventHook(@event, @event,
                                                       IntPtr.Zero, @delegate,
                                                       idProcess, idThread,
                                                       WinEventHookInternalFlags);
        }

        public static bool WinEventUnhook(IntPtr hWinEventHook)
        {
            return UnsafeNativeMethods.UnhookWinEvent(hWinEventHook);
        }

        /// <summary>
        /// 返回 hWnd 窗口的线程标识
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static uint GetWindowThread(IntPtr hWnd)
        {
            return UnsafeNativeMethods.GetWindowThreadProcessId(hWnd, IntPtr.Zero);
        }

        /// <summary>
        /// 传入 out 参数，通过 hWnd 获取 PID
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static uint GetWindowThread(IntPtr hWnd, out uint processId)
        {
            return UnsafeNativeMethods.GetWindowThreadProcessId(hWnd, out processId);
        }

        /// <summary>
        /// 返回窗口左上和右下的坐标(left, top, right, bottom)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static RECT GetWindowRect(IntPtr hWnd)
        {
            var rect = new RECT();
            _ = SafeNativeMethods.GetWindowRect(hWnd, ref rect);
            return rect;
        }

        /// <summary>
        /// 返回窗口客户区大小, Right as Width, Bottom as Height 其余两项只会是0
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static RECT GetClientRect(IntPtr hWnd)
        {
            var rect = new RECT();
            _ = SafeNativeMethods.GetClientRect(hWnd, ref rect);
            return rect;
        }

        /// <summary>
        /// 取得前台窗口句柄函数
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetForegroundWindow()
        {
            return SafeNativeMethods.GetForegroundWindow();
        }

        public static bool DestroyWindow(IntPtr hWnd)
        {
            return SafeNativeMethods.DestroyWindow(hWnd);
        }

        public static int GetWindowLong(IntPtr hWnd, int index)
        {
            return SafeNativeMethods.GetWindowLong(hWnd, index);
        }

        public static int SetWindowLong(IntPtr hWnd, int index, int newStyle)
        {
            return SafeNativeMethods.SetWindowLong(hWnd, index, newStyle);
        }

        public static bool SetForegroundWindow(IntPtr hWnd)
        {
            return SafeNativeMethods.SetForegroundWindow(hWnd);
        }

        public static bool BringWindowToTop(IntPtr hWnd)
        {
            return SafeNativeMethods.BringWindowToTop(hWnd);
        }

        public static IntPtr GetWindow(IntPtr parentHWnd, GW uCmd)
        {
            return SafeNativeMethods.GetWindow(parentHWnd, uCmd);
        }

        public static int GetWindowText(IntPtr hWnd, StringBuilder title, int length)
        {
            return SafeNativeMethods.GetWindowText(hWnd, title, length);
        }

        public static void SwitchToThisWindow(IntPtr hWnd, bool fAltTab = true)
        {
            SafeNativeMethods.SwitchToThisWindow(hWnd, fAltTab);
        }

        public const long SWEH_CHILDID_SELF = 0;

        private static readonly SWEH_dwFlags WinEventHookInternalFlags = SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

        // https://www.jetbrains.com/help/resharper/MemberHidesStaticFromOuterClass.html
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int pid);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
            public static extern bool DestroyWindow(IntPtr hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int index);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool BringWindowToTop(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindow(IntPtr parentHWnd, NativeMethods.GW uCmd);

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll")]
            public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

            [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
            public static extern void KeybdEvent(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

            [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
            public static extern uint SHAppBarMessage(int dwMessage, ref AppBarData pData);

            [DllImport("User32.dll", CharSet = CharSet.Unicode)]
            public static extern int RegisterWindowMessage(string msg);

            [DllImport("user32.dll")]
            public static extern IntPtr GetShellWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
        }

        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr voidProcessId);

            [DllImport("user32.dll", SetLastError = false)]
            public static extern IntPtr SetWinEventHook(NativeMethods.SWEHEvents eventMin, NativeMethods.SWEHEvents eventMax,
                                                        IntPtr hmodWinEventProc, NativeMethods.WinEventDelegate lpfnWinEventProc,
                                                        uint idProcess, uint idThread, NativeMethods.SWEH_dwFlags dwFlags);

            [DllImport("user32.dll", SetLastError = false)]
            public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        }
         
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AppBarData
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        public enum SC : uint
        {
            MAXIMIZE = 0xF030,
            MINIMIZE = 0xF020,
            RESTORE = 0xF120,
        }

        public enum WM : uint
        {
            INACTIVE = 0x0000,
            SETFOCUS = 0x0007,
            KILLFOCUS = 0x0008,
            ACTIVATE = 0x0006,

            SYSCOMMAND = 0x0112,
        }

        public enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }

        public enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        public enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        //SetWinEventHook() flags
        public enum SWEH_dwFlags : uint
        {
            WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
            WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
            WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
            WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
        }

        //SetWinEventHook() Object Ids
        public enum SWEH_ObjectId : long
        {
            OBJID_WINDOW = 0x00000000,
            OBJID_SYSMENU = 0xFFFFFFFF,
            OBJID_TITLEBAR = 0xFFFFFFFE,
            OBJID_MENU = 0xFFFFFFFD,
            OBJID_CLIENT = 0xFFFFFFFC,
            OBJID_VSCROLL = 0xFFFFFFFB,
            OBJID_HSCROLL = 0xFFFFFFFA,
            OBJID_SIZEGRIP = 0xFFFFFFF9,
            OBJID_CARET = 0xFFFFFFF8,
            OBJID_CURSOR = 0xFFFFFFF7,
            OBJID_ALERT = 0xFFFFFFF6,
            OBJID_SOUND = 0xFFFFFFF5,
            OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,
            OBJID_NATIVEOM = 0xFFFFFFF0
        }

        //SetWinEventHook() events
        public enum SWEHEvents : uint
        {
            EventMin = 0x00000001,
            EventMax = 0x7FFFFFFF,
            EventSystemSound = EventMin,
            EventSystemAlert = 0x0002,
            EventSystemForeground = 0x0003,
            EventSystemMenuStart = 0x0004,
            EventSystemMenuEnd = 0x0005,
            EventSystemMenuPopupStart = 0x0006,
            EventSystemMenuPopupEnd = 0x0007,
            EventSystemCaptureStart = 0x0008,
            EventSystemCaptureEnd = 0x0009,
            EventSystemMoveSizeStart = 0x000A,
            EventSystemMoveSizeEnd = 0x000B,
            EventSystemContextHelpStart = 0x000C,
            EventSystemContextHelpEnd = 0x000D,
            EventSystemDragDropStart = 0x000E,
            EventSystemDragDropEnd = 0x000F,
            EventSystemDialogStart = 0x0010,
            EventSystemDialogEnd = 0x0011,
            EventSystemScrollingStart = 0x0012,
            EventSystemScrollingEnd = 0x0013,
            EventSystemSwitchStart = 0x0014,
            EventSystemSwitchEnd = 0x0015,
            EventSystemMinimizeStart = 0x0016,
            EventSystemMinimizeEnd = 0x0017,
            EventSystemDesktopSwitch = 0x0020,
            EventSystemEnd = 0x00FF,
            EventOemDefinedStart = 0x0101,
            EventOemDefinedEnd = 0x01FF,
            EventUiaEventIdStart = 0x4E00,
            EventUiaEventIdEnd = 0x4EFF,
            EventUiaPropIdStart = 0x7500,
            EventUiaPropIdEnd = 0x75FF,
            EventConsoleCaret = 0x4001,
            EventConsoleUpdateRegion = 0x4002,
            EventConsoleUpdateSimple = 0x4003,
            EventConsoleUpdateScroll = 0x4004,
            EventConsoleLayout = 0x4005,
            EventConsoleStartApplication = 0x4006,
            EventConsoleEndApplication = 0x4007,
            EventConsoleEnd = 0x40FF,
            EventObjectCreate = 0x8000,               // hWnd ID idChild is created item
            EventObjectDestroy = 0x8001,              // hWnd ID idChild is destroyed item
            EventObjectShow = 0x8002,                 // hWnd ID idChild is shown item
            EventObjectHide = 0x8003,                 // hWnd ID idChild is hidden item
            EventObjectReorder = 0x8004,              // hWnd ID idChild is parent of z-ordering children
            EventObjectFocus = 0x8005,                // * hWnd ID idChild is focused item
            EventObjectSelection = 0x8006,            // hWnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex
            EventObjectSelectionAdd = 0x8007,         // hWnd ID idChild is item added
            EventObjectSelectionRemove = 0x8008,      // hWnd ID idChild is item removed
            EventObjectSelectionWithin = 0x8009,      // hWnd ID idChild is parent of changed selected items
            EventObjectStateChange = 0x800A,          // hWnd ID idChild is item w/ state change
            EventObjectLocationChange = 0x800B,       // * hWnd ID idChild is moved/sized item
            EventObjectNameChange = 0x800C,           // hWnd ID idChild is item w/ name change
            EventObjectDescriptionChange = 0x800D,    // hWnd ID idChild is item w/ desc change
            EventObjectValueChange = 0x800E,          // hWnd ID idChild is item w/ value change
            EventObjectParentChange = 0x800F,         // hWnd ID idChild is item w/ new parent
            EventObjectHelpChange = 0x8010,           // hWnd ID idChild is item w/ help change
            EventObjectDefactionChange = 0x8011,      // hWnd ID idChild is item w/ def action change
            EventObjectAcceleratorChange = 0x8012,    // hWnd ID idChild is item w/ keybd accel change
            EventObjectInvoked = 0x8013,              // hWnd ID idChild is item invoked
            EventObjectTextSelectionChanged = 0x8014, // hWnd ID idChild is item w? test selection change
            EventObjectContentScrolled = 0x8015,
            EventSystemArrangementPreview = 0x8016,
            EventObjectEnd = 0x80FF,
            EventAiaStart = 0xA000,
            EventAiaEnd = 0xAFFF
        }

        //uCmd 可选值:
        public enum GW : uint
        {
            /// <summary>
            /// 同级别第一个
            /// </summary>
            HWNDFIRST = 0,
            /// <summary>
            /// 同级别最后一个
            /// </summary>
            HWNDLAST = 1,
            /// <summary>
            /// 同级别下一个
            /// </summary>
            HWNDNEXT = 2,
            /// <summary>
            /// 同级别上一个
            /// </summary>
            HWNDPREV = 3,
            /// <summary>
            /// 属主窗口
            /// </summary>
            OWNER = 4,
            /// <summary>
            /// 子窗口
            /// </summary>
            CHILD = 5,
        }
    }
}