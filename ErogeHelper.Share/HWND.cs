using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ErogeHelper.Share
{
    public interface IHandle
    {
        IntPtr DangerousGetHandle();
    }

    public interface IUserHandle : IHandle { }

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{handle}")]
    public struct HWND : IUserHandle
    {
        private readonly IntPtr handle;

        public HWND(IntPtr preexistingHandle) => handle = preexistingHandle;

        public static HWND NULL => new(IntPtr.Zero);

        public bool IsNull => handle == IntPtr.Zero;

        public static HWND HWND_BOTTOM = new IntPtr(1);
        public static HWND HWND_BROADCAST = new IntPtr(0xffff);
        public static HWND HWND_MESSAGE = new IntPtr(-3);
        public static HWND HWND_NOTOPMOST = new IntPtr(-2);
        public static HWND HWND_TOP = new IntPtr(0);
        public static HWND HWND_TOPMOST = new IntPtr(-1);

        public static explicit operator IntPtr(HWND h) => h.handle;
        public static implicit operator HWND(IntPtr h) => new(h);
        public static bool operator !(HWND hMem) => hMem.IsNull;
        public static bool operator !=(HWND h1, HWND h2) => !(h1 == h2);
        public static bool operator ==(HWND h1, HWND h2) => h1.Equals(h2);
        public override bool Equals(object obj) => obj is HWND h && handle == h.handle;
        public override int GetHashCode() => handle.GetHashCode();
        public IntPtr DangerousGetHandle() => handle;
    }
}

