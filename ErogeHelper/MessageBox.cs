using System.Runtime.InteropServices;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper") => _ = MessageBox_(IntPtr.Zero, text, title, 0);

    [DllImport("user32.dll", EntryPoint = "MessageBoxA", ExactSpelling = true, CharSet = CharSet.Ansi)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "SYSLIB1054:コンパイル時に P/Invoke マーシャリング コードを生成するには、'DllImportAttribute' の代わりに 'LibraryImportAttribute' を使用します", Justification = "<保留中>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:P/Invoke 文字列引数に対してマーシャリングを指定します", Justification = "<保留中>")]
    private static extern int MessageBox_(IntPtr hWnd, string lpText, string lpCaption, int uType);
}
