using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Share
{
    public partial class User32
    {
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

        [Flags()]
        public enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000u,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_THICKFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,
            WS_CHILDWINDOW = WS_CHILD,
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


        [Flags]
        public enum WINEVENT
        {
            WINEVENT_INCONTEXT = 0,
            WINEVENT_OUTOFCONTEXT = 1,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_SKIPOWNTHREAD = 4,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HWINEVENTHOOK : IHandle
        {
            private IntPtr handle;
            public HWINEVENTHOOK(IntPtr preexistingHandle) => handle = preexistingHandle;

            public static HWINEVENTHOOK NULL => new HWINEVENTHOOK(IntPtr.Zero);

            public bool IsNull => handle == IntPtr.Zero;

            public static explicit operator IntPtr(HWINEVENTHOOK h) => h.handle;
            
            public static implicit operator HWINEVENTHOOK(IntPtr h) => new HWINEVENTHOOK(h);

            public static bool operator !=(HWINEVENTHOOK h1, HWINEVENTHOOK h2) => !(h1 == h2);

            public static bool operator ==(HWINEVENTHOOK h1, HWINEVENTHOOK h2) => h1.Equals(h2);

            public override bool Equals(object obj) => obj is HWINEVENTHOOK h ? handle == h.handle : false;

            public override int GetHashCode() => handle.GetHashCode();

            public IntPtr DangerousGetHandle() => handle;
        }

        public enum WindowMessage
        {
            WM_SYSCOMMAND = 0x0112,
        }

        public enum SysCommand
        {
            SC_CLOSE = 0xF060
        }

        public enum SetWindowPosFlags : uint
        {
            SWP_NOACTIVATE = 0x0010,

            SWP_NOMOVE = 0x0002,

            SWP_NOSIZE = 0x0001,

            SWP_NOZORDER = 0x0004,

            SWP_SHOWWINDOW = 0x0040,
        }

        public enum HookType
        {
            A = -1,
            WH_MOUSE_LL = 14
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum MOUSEEVENTF
        {
            /// <summary>The left button is down.</summary>
            MOUSEEVENTF_LEFTDOWN = 0x0002,

            /// <summary>The left button is up.</summary>
            MOUSEEVENTF_LEFTUP = 0x0004,

            /// <summary>The right button is down.</summary>
            MOUSEEVENTF_RIGHTDOWN = 0x0008,

            /// <summary>The right button is up.</summary>
            MOUSEEVENTF_RIGHTUP = 0x0010,
        }

        public enum SystemMetric : int
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
        }
    }

    public enum ShowWindowCommand
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
    }


    public partial class User32
    {

        [DllImport(Lib.User32, SetLastError = true, ExactSpelling = true)]
        public static extern bool UnhookWindowsHookEx(HHOOK hhk);

        public class SafeHHOOK : SafeHANDLE, IUserHandle
        {
            /// <summary>Initializes a new instance of the <see cref="SafeHHOOK"/> class and assigns an existing handle.</summary>
            /// <param name="preexistingHandle">An <see cref="IntPtr"/> object that represents the pre-existing handle to use.</param>
            /// <param name="ownsHandle">
            /// <see langword="true"/> to reliably release the handle during the finalization phase; otherwise, <see langword="false"/> (not recommended).
            /// </param>
            public SafeHHOOK(IntPtr preexistingHandle, bool ownsHandle = true) : base(preexistingHandle, ownsHandle) { }

            /// <summary>Initializes a new instance of the <see cref="SafeHHOOK"/> class.</summary>
            private SafeHHOOK() : base() { }

            /// <summary>Performs an implicit conversion from <see cref="SafeHHOOK"/> to <see cref="HHOOK"/>.</summary>
            /// <param name="h">The safe handle instance.</param>
            /// <returns>The result of the conversion.</returns>
            public static implicit operator HHOOK(SafeHHOOK h) => h.handle;


            /// <inheritdoc/>
            protected override bool InternalReleaseHandle() => UnhookWindowsHookEx(this);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HHOOK : IUserHandle
        {
            private IntPtr handle;

            /// <summary>Initializes a new instance of the <see cref="HHOOK"/> struct.</summary>
            /// <param name="preexistingHandle">An <see cref="IntPtr"/> object that represents the pre-existing handle to use.</param>
            public HHOOK(IntPtr preexistingHandle) => handle = preexistingHandle;

            /// <summary>Returns an invalid handle by instantiating a <see cref="HHOOK"/> object with <see cref="IntPtr.Zero"/>.</summary>
            public static HHOOK NULL => new HHOOK(IntPtr.Zero);

            /// <summary>Gets a value indicating whether this instance is a null handle.</summary>
            public bool IsNull => handle == IntPtr.Zero;

            /// <summary>Performs an explicit conversion from <see cref="HHOOK"/> to <see cref="IntPtr"/>.</summary>
            /// <param name="h">The handle.</param>
            /// <returns>The result of the conversion.</returns>
            public static explicit operator IntPtr(HHOOK h) => h.handle;

            /// <summary>Performs an implicit conversion from <see cref="IntPtr"/> to <see cref="HHOOK"/>.</summary>
            /// <param name="h">The pointer to a handle.</param>
            /// <returns>The result of the conversion.</returns>
            public static implicit operator HHOOK(IntPtr h) => new HHOOK(h);

            /// <summary>Implements the operator !=.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(HHOOK h1, HHOOK h2) => !(h1 == h2);

            /// <summary>Implements the operator ==.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(HHOOK h1, HHOOK h2) => h1.Equals(h2);

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is HHOOK h ? handle == h.handle : false;

            /// <inheritdoc/>
            public override int GetHashCode() => handle.GetHashCode();

            /// <inheritdoc/>
            public IntPtr DangerousGetHandle() => handle;
        }

        public abstract class SafeHANDLE : SafeHandleZeroOrMinusOneIsInvalid, IEquatable<SafeHANDLE>, IHandle
        {
            /// <summary>Initializes a new instance of the <see cref="SafeHANDLE"/> class.</summary>
            public SafeHANDLE() : base(true)
            {
            }

            /// <summary>Initializes a new instance of the <see cref="SafeHANDLE"/> class and assigns an existing handle.</summary>
            /// <param name="preexistingHandle">An <see cref="IntPtr"/> object that represents the pre-existing handle to use.</param>
            /// <param name="ownsHandle">
            /// <see langword="true"/> to reliably release the handle during the finalization phase; otherwise, <see langword="false"/> (not recommended).
            /// </param>
            protected SafeHANDLE(IntPtr preexistingHandle, bool ownsHandle = true) : base(ownsHandle) => SetHandle(preexistingHandle);

            /// <summary>Gets a value indicating whether this instance is null.</summary>
            /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
            public bool IsNull => handle == IntPtr.Zero;

            /// <summary>Implements the operator !=.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(SafeHANDLE h1, IHandle h2) => !(h1 == h2);

            /// <summary>Implements the operator ==.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(SafeHANDLE h1, IHandle h2) => h1?.Equals(h2) ?? h2 is null;

            /// <summary>Implements the operator !=.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(SafeHANDLE h1, IntPtr h2) => !(h1 == h2);

            /// <summary>Implements the operator ==.</summary>
            /// <param name="h1">The first handle.</param>
            /// <param name="h2">The second handle.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(SafeHANDLE h1, IntPtr h2) => h1?.Equals(h2) ?? false;

            /// <summary>Determines whether the specified <see cref="SafeHANDLE"/>, is equal to this instance.</summary>
            /// <param name="other">The <see cref="SafeHANDLE"/> to compare with this instance.</param>
            /// <returns><c>true</c> if the specified <see cref="SafeHANDLE"/> is equal to this instance; otherwise, <c>false</c>.</returns>
            public bool Equals(SafeHANDLE other)
            {
                if (other is null)
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return handle == other.handle && IsClosed == other.IsClosed;
            }

            /// <summary>Determines whether the specified <see cref="System.Object"/>, is equal to this instance.</summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
            /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case IHandle ih:
                        return handle.Equals(ih.DangerousGetHandle());
                    case SafeHandle sh:
                        return handle.Equals(sh.DangerousGetHandle());
                    case IntPtr p:
                        return handle.Equals(p);
                    default:
                        return base.Equals(obj);
                }
            }

            /// <summary>Returns a hash code for this instance.</summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode() => base.GetHashCode();

            /// <summary>Releases the ownership of the underlying handle and returns the current handle.</summary>
            /// <returns>The value of the current handle.</returns>
            public IntPtr ReleaseOwnership()
            {
                var ret = handle;
                SetHandleAsInvalid();
                return ret;
            }

            /// <summary>
            /// Internal method that actually releases the handle. This is called by <see cref="ReleaseHandle"/> for valid handles and afterwards
            /// zeros the handle.
            /// </summary>
            /// <returns><c>true</c> to indicate successful release of the handle; <c>false</c> otherwise.</returns>
            protected abstract bool InternalReleaseHandle();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
            protected override bool ReleaseHandle()
            {
                if (IsInvalid) return true;
                if (!InternalReleaseHandle()) return false;
                handle = IntPtr.Zero;
                return true;
            }
        }
    }
}
