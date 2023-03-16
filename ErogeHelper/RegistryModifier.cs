using Microsoft.Win32;

namespace ErogeHelper
{
    // .net need target to <TargetFramework>netX.0-windows</TargetFramework>
    internal class RegistryModifier
    {
        private const string ApplicationCompatibilityRegistryPath =
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        public static bool IsDpiCompatibilitySetted(string exeFilePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(ApplicationCompatibilityRegistryPath, true)
                ?? Registry.CurrentUser.CreateSubKey(ApplicationCompatibilityRegistryPath);

            var currentValue = key.GetValue(exeFilePath) as string;
            if (currentValue is null)
                return false;

            var DpiSettings = new string[3] { "HIGHDPIAWARE", "DPIUNAWARE", "GDIDPISCALING DPIUNAWARE" };
            var currentValueList = currentValue.Split(' ');
            return DpiSettings.Any(v => currentValueList.Contains(v));
        }
        public static void SetDPICompatibilityAsApplication(string exeFilePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(ApplicationCompatibilityRegistryPath, true)
                ?? Registry.CurrentUser.CreateSubKey(ApplicationCompatibilityRegistryPath);

            var currentValue = key.GetValue(exeFilePath) as string;
            if (string.IsNullOrEmpty(currentValue))
                key.SetValue(exeFilePath, "~ HIGHDPIAWARE");
            else
            {
                key.SetValue(exeFilePath, currentValue + " HIGHDPIAWARE");
            }
        }

        // Administor is needed
        private const string EdgeUIRegistryPath = "SOFTWARE\\Policies\\Microsoft\\Windows\\EdgeUI";
        private const string AllowEdgeSwipeKey = "AllowEdgeSwipe";

        public static bool IsEdgeUIBlockSet()
        {
            using var key = Registry.LocalMachine.OpenSubKey(EdgeUIRegistryPath, true)
                ?? Registry.LocalMachine.CreateSubKey(EdgeUIRegistryPath);

            var currentValue = key.GetValue(AllowEdgeSwipeKey);
            if (currentValue is int blockValue)
            {
                // 0 blocked, 1 normal
                return blockValue == 0;
            }

            return false;
        }

        public static void SetEdgeUIBlock(bool value)
        {
            using var key = Registry.LocalMachine.OpenSubKey(EdgeUIRegistryPath, true)
                ?? Registry.LocalMachine.CreateSubKey(EdgeUIRegistryPath);

            key.SetValue(AllowEdgeSwipeKey, value);
        }
    }
}
