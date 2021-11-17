using System;
using System.IO;
using ErogeHelper.Share.Entities;
using ErogeHelper.Share.Enums;

namespace ErogeHelper.Share.Contracts
{
    public static class EhContext
    {
        public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string EhDataDir = Path.Combine(RoamingPath, "ErogeHelper");

        public static readonly string EhConfigFilePath = Path.Combine(EhDataDir, "EhSettings.json");

        public static readonly string EhDbFilePath = Path.Combine(EhDataDir, "eh.db");

        public static readonly string DbConnectString = $"Data Source={EhDbFilePath}";
        
        public static readonly AssistiveTouchPosition TouchPosition = new(TouchButtonCorner.Left, 0.5);

        public const double AssistiveTouchSize = 55;
        public const double AssistiveTouchSizeBig = 100;
    }
}

