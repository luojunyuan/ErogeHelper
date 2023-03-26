using ErogeHelper.AssistiveTouch.Helper;
using System.IO;
using System.Windows;

namespace ErogeHelper.AssistiveTouch
{
    internal static class Config
    {
        private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigFolder = Path.Combine(RoamingPath, "ErogeHelper");
        private static readonly string ConfigFilePath = Path.Combine(RoamingPath, "ErogeHelper", "EHConfig.ini");

        public static bool EnterKeyMapping { get; private set; }

        public static bool ScreenShotTradition { get; private set; }

        public static string AssistiveTouchPosition { get; private set; } = string.Empty;

        public static void Load()
        {
            // First time start
            if (!File.Exists(ConfigFilePath))
                return;

            var myIni = new IniFile(ConfigFilePath);
            EnterKeyMapping = bool.Parse(myIni.Read(nameof(EnterKeyMapping)) ?? "false");
            ScreenShotTradition = bool.Parse(myIni.Read(nameof(ScreenShotTradition)) ?? "false");
            AssistiveTouchPosition = myIni.Read(nameof(AssistiveTouchPosition)) ?? string.Empty;
        }

        public static void SaveAssistiveTouchPosition(string pos)
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            var myIni = new IniFile(ConfigFilePath);
            myIni.Write(nameof(AssistiveTouchPosition), pos);
        }
    }
    // MappingKey
}
