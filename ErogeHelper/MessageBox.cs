using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ErogeHelper;

internal class MessageBox
{
    public static void Show(string text, string title = "ErogeHelper", IntPtr parent = default) => _ = AppLauncher.User32.MessageBox(parent, text, title, MB_TOPMOST | MB_SETFOREGROUND);

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
