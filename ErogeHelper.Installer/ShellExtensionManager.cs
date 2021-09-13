using Microsoft.Win32;

namespace ErogeHelper.Installer
{
    internal static class ShellExtensionManager
    {
        //private static readonly string keyName = $@"CLSID\{{FC0DDE3F-C236-3705-8E20-1BEF78D62D0B}}";
        private const string FriendlyName = "ErogeHelper.ShellMenuHandler.ShellMenuExtension";

        public static bool IsInstalled()
        {
            var rootName = Registry.ClassesRoot;

            return rootName.OpenSubKey(FriendlyName, false) is not null;
        }
    }
}
