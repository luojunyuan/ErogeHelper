using System.Runtime.InteropServices;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper") => _ = MessageBox_(IntPtr.Zero, text, title, MB_TOPMOST);

    [DllImport("user32.dll", EntryPoint = "MessageBoxA", ExactSpelling = true, CharSet = CharSet.Ansi)]
    private static extern int MessageBox_(IntPtr hWnd, string lpText, string lpCaption, int uType);

    const int MB_TOPMOST = 0x00040000;
    // https://stackoverflow.com/a/48003123
}
