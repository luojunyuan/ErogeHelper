using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Installer
{
    static class ShellExtensionManager
    {
        //private static readonly string keyName = $@"CLSID\{{FC0DDE3F-C236-3705-8E20-1BEF78D62D0B}}";
        private static readonly string friendlyName = "ErogeHelper.ShellMenuHandler.ShellMenuExtension";

        public static bool IsInstalled()
        {
            var rootName = Registry.ClassesRoot;
            
            return rootName.OpenSubKey(friendlyName, false) != null;
        }
    }
}
