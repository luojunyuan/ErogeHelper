using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ErogeHelper.Shared;

public static class Utils
{
    private static readonly Version OsVersion = new(Environment.OSVersion.Version.Major,
                                                    Environment.OSVersion.Version.Minor,
                                                    Environment.OSVersion.Version.Build);

    public static bool IsWin7 { get; } = OsVersion < new Version(6, 2);

    public static bool IsWin8 { get; } = !IsWin7 && OsVersion <= new Version(6, 3);

    public static bool HasWinRT { get; } = OsVersion > new Version(10, 0);

    public static bool IsArm { get; } = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    public static bool IsFileInUse(string fileName)
    {
        var inUse = true;

        FileStream? fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            inUse = false;
        }
        catch { /* ignored */ }
        finally
        {
            fs?.Close();
        }
        return inUse;
    }

    public static List<Process> GetProcessesByFriendlyName(string friendlyName)
    {
        var processes = new List<Process>();
        processes.AddRange(Process.GetProcessesByName(friendlyName));
        processes.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
        if (!friendlyName.Equals("main.bin", StringComparison.Ordinal))
            processes.AddRange(Process.GetProcessesByName("main.bin"));
        return processes;
    }

    public static string GetOsInfo()
    {
        var windows7 = new Version(6, 1);
        var windows8 = new Version(6, 2);
        var windows81 = new Version(6, 3);
        var windows10 = new Version(10, 0);
        var windows11 = new Version(10, 0, 22000);

        var architecture = RuntimeInformation.ProcessArchitecture;
        var buildVersion = Environment.OSVersion.Version.Build;
        var releaseId = buildVersion switch
        {
            22000 => "21H2",
            19044 => "21H2",
            19043 => "21H1",
            19042 => "20H2",
            19041 => "2004",
            18363 => "1909",
            18362 => "1903", // Current target WinRT-SDK version
            17763 => "1809",
            17134 => "1803",
            16299 => "1709",
            15063 => "1703",
            14393 => "1607",
            10586 => "1511",
            10240 => "1507",
            _ => buildVersion.ToString()
        };

        // Not reliable
        // var osName = Registry.GetValue(ConstantValues.HKLMWinNTCurrent, "productName", "")?.ToString();

        var windowVersionString =
            OsVersion >= windows11 ? $"Windows 11 {releaseId}" :
            OsVersion >= windows10 ? $"Windows 10 {releaseId}" :
            OsVersion >= windows81 ? "Windows 8.1" :
            OsVersion >= windows8 ? "Windows 8" :
            OsVersion >= windows7 ? $"Windows 7 {Environment.OSVersion.ServicePack}" :
            Environment.OSVersion.VersionString;

        return $"{windowVersionString} {architecture}";
    }

    public static string Md5Calculate(string str, bool toUpper = false) =>
        Md5Calculate(str, Encoding.Default, toUpper);

    private static string Md5Calculate(string str, Encoding encoding, bool toUpper = false)
    {
        var buffer = encoding.GetBytes(str);
        return Md5Calculate(buffer, toUpper);
    }

    public static string Md5Calculate(byte[] buffer, bool toUpper = false)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(buffer);

        var sb = new StringBuilder();

        var format = toUpper ? "X2" : "x2";

        foreach (var byteItem in hash)
        {
            sb.Append(byteItem.ToString(format));
        }

        return sb.ToString();
    }
}
