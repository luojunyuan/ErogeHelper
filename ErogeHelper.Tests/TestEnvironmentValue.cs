using System;

namespace ErogeHelper.Tests
{
    public static class TestEnvironmentValue
    {
        public static string RoamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}