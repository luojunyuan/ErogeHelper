namespace ErogeHelper.Common.Contracts
{
    public static class ConstantValues
    {
        public const string SerilogOutputTemplate =
            "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public const int TextractorReAttachBlockTime = 100;

        public const int WaitNWJSGameStartDelay = 7000;

        public const int WaitGameStartTimeout = 20000;

        public const int MinimumLagTime = 50;

        public const int GameWindowStatusRefreshTime = 200;

        public const int ToastDuration = 5000;

        public const string UWPAppsTag = "WindowsApps";

        public const string WindowsPath = @"C:\Windows\";
    }
}
