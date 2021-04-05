using System;

namespace ErogeHelper.Tests
{
    public static class TestEnvironmentValue
    {
        public static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}