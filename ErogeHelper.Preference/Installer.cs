using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace ErogeHelper.Preference
{
    internal static class Installer
    {
        private static readonly string CrtDir = Path.GetDirectoryName(typeof(Installer).Assembly.Location);
        private static readonly string RegisterHandlePath = Path.Combine(CrtDir, "EHContextMenuHandler.dll");

        public static void DoRegister(bool allUsers)
        {
            try
            {
                if (!allUsers)
                    OverrideHKCR();

                var rs = new RegistrationServices();
                rs.RegisterAssembly(Assembly.LoadFrom(RegisterHandlePath), AssemblyRegistrationFlags.SetCodeBase);

                ShellExtensionManager.RegisterShellExtContextMenuHandler(allUsers);

                if (!allUsers)
                    OverrideHKCR(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.MainWindow, e.Message + "\r\n\r\n" + e.StackTrace);
            }
        }

        public static void DoUnRegister(bool allUsers)
        {
            try
            {
                if (!allUsers)
                    OverrideHKCR();

                var rs = new RegistrationServices();
                rs.UnregisterAssembly(Assembly.LoadFrom(RegisterHandlePath));

                ShellExtensionManager.UnregisterShellExtContextMenuHandler(allUsers);

                if (!allUsers)
                    OverrideHKCR(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.MainWindow, e.Message + "\r\n\r\n" + e.StackTrace);
            }
        }

        public static void NotifyShell()
        {
            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const ushort SHCNF_IDLIST = 0x0000;

            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, ushort uFlags, IntPtr dwItem1, IntPtr dwItem2);

        #region OverrideHKCR
      
        private static void OverrideHKCR(bool restore = false)
        {
            UIntPtr HKEY_CLASSES_ROOT = Is64BitOS() ? new UIntPtr(0xFFFFFFFF80000000) : new UIntPtr(0x80000000);
            UIntPtr HKEY_CURRENT_USER = Is64BitOS() ? new UIntPtr(0xFFFFFFFF80000001) : new UIntPtr(0x80000001);

            // 0xF003F = KEY_ALL_ACCESS
            RegOpenKeyEx(HKEY_CURRENT_USER, @"Software\Classes", 0, 0xF003F, out UIntPtr key);
            RegOverridePredefKey(HKEY_CLASSES_ROOT, restore ? UIntPtr.Zero : key);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, uint samDesired,
           out UIntPtr hkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegOverridePredefKey(UIntPtr hKey, UIntPtr hNewKey);

        private static bool Is64BitOS()
        {
            //The code below is from http://1code.codeplex.com/SourceControl/changeset/view/39074#842775
            //which is under the Microsoft Public License: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.

            if (UIntPtr.Size == 8) // 64-bit programs run only on Win64
            {
                return true;
            }
            // Detect whether the current process is a 32-bit process 
            // running on a 64-bit system.
            bool flag;
            return DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                   IsWow64Process(GetCurrentProcess(), out flag) && flag;
        }
        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            var moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == UIntPtr.Zero)
            {
                return false;
            }
            return GetProcAddress(moduleHandle, methodName) != UIntPtr.Zero;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(UIntPtr hProcess, out bool wow64Process);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern UIntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern UIntPtr GetProcAddress(UIntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        #endregion
    }

    /// <summary>
    /// Read Write Registry
    /// </summary>
    public class ShellExtensionManager
    {
        private static readonly string Clsid = "{C6D31501-E884-457E-904D-762ACC4C7E34}";
        private static readonly string FileType = "*";
        private static readonly string FriendlyName = "ErogeHelper.EHContextMenuHandler Class";
        private static readonly string KeyName = $@"Software\Classes\{FileType}\shellex\ContextMenuHandlers\{Clsid}";

        public static void RegisterShellExtContextMenuHandler(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            using var key = rootName.CreateSubKey(KeyName);
            key?.SetValue(null, FriendlyName);
        }

        public static void UnregisterShellExtContextMenuHandler(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            rootName.DeleteSubKeyTree(KeyName);
        }

        public static bool IsInstalled(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            return rootName.OpenSubKey(KeyName, false) != null;
        }
    }
}
