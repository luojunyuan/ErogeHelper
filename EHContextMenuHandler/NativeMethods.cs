using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace EHContextMenuHandler;

internal class NativeMethods
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder? pszFile, int cch);

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, [MarshalAs(UnmanagedType.Bool)] bool fByPosition, ref MENUITEMINFO mii);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreatePopupMenu();

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hObject);

    public static int HighWord(int number)
    {
        return (number & 0x80000000) == 0x80000000
                   ? number >> 16
                   : (number >> 16) & 0xffff;
    }

    public static int LowWord(int number)
    {
        return number & 0xffff;
    }
}

internal static class WinError
{
    public const int S_OK = 0x0000;
    public const int S_FALSE = 0x0001;
    public const int E_FAIL = -2147467259;
    public const int E_INVALIDARG = -2147024809;
    public const int E_OUTOFMEMORY = -2147024882;
    public const int STRSAFE_E_INSUFFICIENT_BUFFER = -2147024774;
    public const uint SEVERITY_SUCCESS = 0;
    public const uint SEVERITY_ERROR = 1;

    public static int MAKE_HRESULT(uint sev, uint fac, uint code)
    {
        return (int)((sev << 31) | (fac << 16) | code);
    }
}
