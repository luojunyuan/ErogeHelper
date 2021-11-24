using System;
using System.IO;
using ErogeHelper.Share.Entities;
using ErogeHelper.Share.Enums;

namespace ErogeHelper.Share.Contracts
{
    public static class EHContext
    {
        public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string EHDataFolder = Path.Combine(RoamingPath, "ErogeHelper");

        public static readonly string EHConfigFilePath = Path.Combine(EHDataFolder, "EHSettings.json");

        public static readonly string EHDbFilePath = Path.Combine(EHDataFolder, "eh.db");

        public static readonly string DbConnectString = $"Data Source={EHDbFilePath}";
        
        public static readonly AssistiveTouchPosition TouchPosition = new(TouchButtonCorner.Left, 0.5);

        public const double AssistiveTouchSize = 55;
        public const double AssistiveTouchSizeBig = 100;
    }
}

