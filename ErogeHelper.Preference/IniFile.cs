// CODE FROM https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace ErogeHelper.Preference
{
    class IniFile   // revision 11
    {

        private const string LERegistryPath = @"Software\Classes\CLSID\{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}\InprocServer32";
        public static string LEPath()
        {
            using var key = Registry.CurrentUser.OpenSubKey(LERegistryPath) ??
                Registry.LocalMachine.OpenSubKey(LERegistryPath);
            if (key is null)
                return string.Empty;

            var rawPath = key.GetValue("CodeBase") as string;
            if (rawPath is null)
                return string.Empty;

            var handleDllPath = rawPath.Substring(8);
            var dir = Path.GetDirectoryName(handleDllPath);
            if (dir is null)
                return string.Empty;

            return Path.Combine(dir, "LEProc.exe");
        }

        string path;
        string EXE = "ErogeHelper"; // Assembly.GetExecutingAssembly().GetName().Name!;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string? Key, string? Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string? IniPath = null)
        {
            path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        public string? Read(string Key, string? Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255,path);
            return RetVal.ToString() == string.Empty ? null : RetVal.ToString();
        }

        public void Write(string? Key, string? Value, string? Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, path);
        }

        public void DeleteKey(string Key, string? Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string? Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string? Section = null)
        {
            return (Read(Key, Section) ?? string.Empty).Length > 0;
        }
    }
}
