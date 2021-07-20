using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ErogeHelper.Installer
{
    internal static class Win32ErrorHandlingExtensions
    {
        public static int CheckError(this int value)
        {
            return value is not 0 ? value : throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
