namespace ErogeHelper.Common.Contracts
{
    public static class ConstantValues
    {
        public const string SerilogOutputTemplate =
            "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public const string ApplicationCompatibilityRegistryPath =
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

        public const string HKLMWinNTCurrent = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public const int TextractorReAttachBlockTime = 100;

        public const int WaitNWJSGameStartDelay = 7000;

        public const int WaitGameStartTimeout = 20000;

        public const int MinimumLagTime = 50;

        public const int KeyboardLagTime = 25;

        public const int PressFirstKeyLagTime = 500;

        public const int GameWindowStatusRefreshTime = 200;

        public const int ToastDuration = 5000;

        public const int UserOperationDelay = 500;

        public const string UWPAppsTag = "WindowsApps";

        public const string WindowsPath = @"C:\Windows\";

        public const string EhCloudSavedataTag = "eh-savedata-cloud";

        public const string EhCloudDbFilename = "gamelist.json";

        public const int GoodWindowHeight = 100; // 240;

        public const int GoodWindowWidth = 100; // 320;

        public const string DefaultAssistiveTouchPositionStr = "{\"Corner\":0,\"Scale\":0.5}";

        public const int AssistiveTouchOpacityChangedTimeout = 5000;

        public const int ScreenShotHideButtonTime = 3000;

        public const string TaskQueueContentDialogKey = "ContentDialog";
    }
}
