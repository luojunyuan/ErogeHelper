using Microsoft.Win32;
using ErogeHelper.Model.Services.Interface;

namespace ErogeHelper.Platform.Windows;

internal class WindowsGuid : IWindowsGuid
{
    public string MachineGuid { get; } = GetMachineGuid();

    private static string GetMachineGuid()
    {
        if (Environment.Is64BitOperatingSystem)
        {
            var keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var keyX64 = keyBaseX64.OpenSubKey(
                @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            var resultObjX64 = keyX64?.GetValue("MachineGuid", string.Empty);

            if (resultObjX64 is not null)
            {
                return resultObjX64.ToString() ?? string.Empty;
            }
        }
        else
        {
            var keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var keyX86 = keyBaseX86.OpenSubKey(
                @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            var resultObjX86 = keyX86?.GetValue("MachineGuid", string.Empty);

            if (resultObjX86 != null)
            {
                return resultObjX86.ToString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
