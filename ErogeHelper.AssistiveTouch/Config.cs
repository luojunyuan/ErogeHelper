using ErogeHelper.AssistiveTouch.Helper;
using System.IO;

namespace ErogeHelper.AssistiveTouch
{
    internal static class Config
    {
        private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigFilePath = Path.Combine(RoamingPath, "ErogeHelper", "EHConfig.ini");

        public static bool MappingEnter { get; private set; }

        public static void Load()
        {
            // First time start
            if (!File.Exists(ConfigFilePath))
                return;

            var myIni = new IniFile(ConfigFilePath);
            MappingEnter = bool.Parse(myIni.Read(nameof(MappingEnter)) ?? "false");
        }

        public static void Save()
        {

        }
    }
    // ScreenShot way

    // MappingKey

    // TouchPosition
}
