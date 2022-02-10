namespace ErogeHelper.Shared.Contracts;

public static class ConstantValue
{
    public const string SerilogOutputTemplate =
        "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    public const string ApplicationCompatibilityRegistryPath =
        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

    public const string HKLMWinNTCurrent = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    public const int UserTimerMinimum = 0xA;

    public const int UIMinimumResponseTime = 50;

    public const int TextractorReAttachBlockTime = 100;

    public const int UserConfigOperationDelayTime = 500;

    public const int WaitMainWindowActualSizeTime = 1000;

    public const int ScreenShotHideButtonTime = 3000;

    public const int FullscreenDelayRefreshTime = 5000;

    public const int WaitGameStartTimeout = 20000;

    public const string UWPAppsTag = "WindowsApps";

    public const string WindowsPath = @"C:\Windows\";

    public const string CloudSaveDataTag = "eh-savedata-cloud";

    public const string CloudDbFilename = "gamelist.json";

    public const int GoodWindowHeight = 100; // 240;

    public const int GoodWindowWidth = 100; // 320;

    public const string DefaultAssistiveTouchPositionStr = "{\"Corner\":0,\"Scale\":0.5}";

    public const string TaskQueueContentDialogKey = "ContentDialog";

    public const string MeCabUniDicMD5 = "92C3E76ABBB89274EE95A9B8698E691E";

    // HCode 0或1个/ H 1个以上任意字符 @ 1个以上十六进制 (: 1个以上任意字符)
    // RCode 0或1个/ RS@ 1个以上十六进制
    public const string CodeRegExp = @"/?H\S+@[A-Fa-f0-9]+(:\S+)?|/?RS@[A-Fa-f0-9]+";

    public const string AniclanBaseUrl = "http://vnr.aniclan.com";
}
