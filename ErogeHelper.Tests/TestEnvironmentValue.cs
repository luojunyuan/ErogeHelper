using System;
using System.IO;

namespace ErogeHelper.Tests
{
    public static class TestEnvironmentValue
    {
        public static readonly string RoamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string ConnectionString = $"Data Source={Path.Combine(RoamingDir, "ErogeHelper", "eh.db")}";
    }
}