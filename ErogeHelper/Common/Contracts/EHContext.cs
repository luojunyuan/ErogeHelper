using System;
using System.IO;
using System.Reflection;

namespace ErogeHelper.Common.Contracts
{
    internal static class EHContext
    {
        public static readonly string? AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string EHDataDir = Path.Combine(RoamingPath, "ErogeHelper");

        public static readonly string EHConfigFilePath = Path.Combine(EHDataDir, "EhSettings.json");

        public static readonly string EHDBFilePath = Path.Combine(EHDataDir, "eh.db");

        public static readonly string DBConnectString = $"Data Source={EHDBFilePath}";
    }
}
