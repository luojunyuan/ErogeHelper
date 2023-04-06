using System.Runtime.InteropServices;
using System.Text;
using SplashScreenGdip;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper", IntPtr parent = default) => _ = _MessageBox(parent, text, title, MB_TOPMOST | MB_SETFOREGROUND);

    public static void ShowX(string text, SplashScreen splash)
    {
        splash.Hide();
        _MessageBox(splash.WindowHandle, text, "ErogeHelper", MB_TOPMOST | MB_SETFOREGROUND);
        splash.Close();
    }

    [DllImport("user32.dll", EntryPoint = "MessageBoxA", ExactSpelling = true, CharSet = CharSet.Ansi)]
    public static extern int _MessageBox(IntPtr hWnd, string lpText, string lpCaption, int uType);

    // Not work if the process had win handle
    const int MB_TOPMOST = 0x00040000;
    const int MB_SETFOREGROUND = 0x00010000;
    // https://stackoverflow.com/a/48003123
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

    internal class Kernel32
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
    }
}
