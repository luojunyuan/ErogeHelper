using System;
using System.IO;
using ErogeHelper.Share.Entities;
using ErogeHelper.Share.Enums;

namespace ErogeHelper.Share.Contracts
{
    public static class EHContext
    {
        public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string RoamingEHFolder = Path.Combine(RoamingPath, "ErogeHelper");

        public static readonly string ConfigFilePath = Path.Combine(RoamingEHFolder, "EHSettings.json");

        public static readonly string DbFilePath = Path.Combine(RoamingEHFolder, "eh.db");

        public static readonly string DbConnectString = $"Data Source={DbFilePath}";
        
        public static readonly AssistiveTouchPosition TouchPosition = new(TouchButtonCorner.Left, 0.5);

        public const double AssistiveTouchSize = 55;
        public const double AssistiveTouchSizeBig = 100;
    }
}

