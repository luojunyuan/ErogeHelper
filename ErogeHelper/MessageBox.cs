using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper", IntPtr parent = default) => _ = User32.MessageBox(parent, text, title, MB_TOPMOST | MB_SETFOREGROUND);

    // Not work if the process had win handle
    const int MB_TOPMOST = 0x00040000;
    const int MB_SETFOREGROUND = 0x00010000;
    // https://stackoverflow.com/a/48003123
}

internal class User32
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "MessageBoxA", ExactSpelling = true, CharSet = CharSet.Ansi)]
    public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, int uType);

    private const string User32Dll = "user32.dll";

    [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool EnumWindowsProc([In] IntPtr hwnd, [In] IntPtr lParam);

    [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport(User32Dll, SetLastError = false, ExactSpelling = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport(User32Dll, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int Width
        {
            get { return right - left; }
            set { right = value + left; }
        }

        public int Height
        {
            get { return bottom - top; }
            set { bottom = value + top; }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
            set { Width = value.Width; Height = value.Height; }
        }
    }

    public enum ShowWindowCommand
    {
        SW_RESTORE = 9,
    }
}

public class AppdataRoming
{
    private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string ConfigFilePath = Path.Combine(RoamingPath, "ErogeHelper", "EHConfig.ini");

    public static bool IsDpiAppDisabled()
    {
        var valueBuilder = new StringBuilder(255);
        Kernel32.GetPrivateProfileString("ErogeHelper", "DpiAppDisabled", string.Empty, valueBuilder, 255, ConfigFilePath);
        if (valueBuilder.ToString() == string.Empty)
            return false;
        return bool.Parse(valueBuilder.ToString());
    }
}

internal class Kernel32
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
}
