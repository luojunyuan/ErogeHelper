using System.IO;
using System.Reflection;

namespace ErogeHelper.Function;

public static class EHContext
{
    private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static readonly string RoamingFolder = Path.Combine(RoamingPath, "ErogeHelper");

    public static readonly string ConfigFilePath = Path.Combine(RoamingFolder, "EHSettings.json");
    public static string EHVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "9.9.9.9";

    public const int UserTimerMinimum = 0xA;
    public const int UIMinimumResponseTime = 50;
    public readonly static TimeSpan UserConfigOperationDelay = TimeSpan.FromMilliseconds(500);
}
