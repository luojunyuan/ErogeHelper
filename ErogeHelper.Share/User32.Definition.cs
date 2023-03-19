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



        public enum SystemMetric : int
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
        }
    }



    public partial class User32
    {

       
    }
}
