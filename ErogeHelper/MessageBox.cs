using System.Runtime.InteropServices;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper", IntPtr parent = default) => _ = MessageBox_(parent, text, title, MB_TOPMOST | MB_SETFOREGROUND);

    [DllImport("user32.dll", EntryPoint = "MessageBoxA", ExactSpelling = true, CharSet = CharSet.Ansi)]
    private static extern int MessageBox_(IntPtr hWnd, string lpText, string lpCaption, int uType);

    const int MB_SETFOREGROUND = 0x00010000;
    const int MB_TOPMOST = 0x00040000;
    // https://stackoverflow.com/a/48003123
}

internal class User32
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
