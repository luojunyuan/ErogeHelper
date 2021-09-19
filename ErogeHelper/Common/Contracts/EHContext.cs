using System;
using System.IO;
using System.Reflection;

namespace ErogeHelper.Common.Contracts
{
    internal static class EhContext
    {
        public static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?.?";

        public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string EhDataDir = Path.Combine(RoamingPath, "ErogeHelper");

        public static readonly string EhConfigFilePath = Path.Combine(EhDataDir, "EhSettings.json");

        public static readonly string EhDbFilePath = Path.Combine(EhDataDir, "eh.db");

        public static readonly string DbConnectString = $"Data Source={EhDbFilePath}";
    }
}
