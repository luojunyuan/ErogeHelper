using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ErogeHelper;

public static class Utils
{
    private static readonly Version OsVersion = new(Environment.OSVersion.Version.Major,
                                                    Environment.OSVersion.Version.Minor,
                                                    Environment.OSVersion.Version.Build);

    /// <summary>
    /// System begin with the first win10 1507 build 10240
    /// </summary>
    public static bool HasWinRT { get; } = OsVersion > new Version(10, 0);

    /// <summary>
    /// XamlIslands can be used
    /// </summary>
    public static bool IsOrAfter1903 { get; } = OsVersion >= new Version(10, 0, 18362);

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
            18362 => "1903", // Target winrt-sdk version
            17763 => "1809",
            17134 => "1803",
            16299 => "1709",
            15063 => "1703",
            14393 => "1607",
            10586 => "1511",
            10240 => "1507",
            _ => buildVersion.ToString()
        };

        // osName not reliable
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

    public static List<Process> GetProcessesByFriendlyName(string friendlyName)
    {
        var processes = new List<Process>();
        processes.AddRange(Process.GetProcessesByName(friendlyName));
        processes.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
        if (!friendlyName.Equals("main.bin", StringComparison.Ordinal))
            processes.AddRange(Process.GetProcessesByName("main.bin"));
        return processes;
    }

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

    public static string TextEvaluateWrapperWithRegExp(string sourceInput, string expr)
    {
        const string begin = "|~S~|";
        const string end = "|~E~|";

        if (expr == string.Empty)
            return sourceInput;

        //if (expr.AsSpan()[^1] == '|')
        //    return sourceInput;

        string wrapperText = sourceInput;

        var instant = new Regex(expr); // asd\
        var collect = instant.Matches(sourceInput);
        foreach (Match match in collect)
        {
            var beginPos = wrapperText.LastIndexOf(end, StringComparison.Ordinal);
            wrapperText = instant.Replace(
                wrapperText,
                begin + match + end,
                1,
                beginPos == -1 ? 0 : beginPos + 5);
        }
        return wrapperText;
    }

    public static string Md5Calculate(string path, bool toUpper = false) =>
        Md5Calculate(File.ReadAllBytes(path), toUpper);

    /// <param name="buffer">May allocate a free space at LOH of buffer size</param>
    private static string Md5Calculate(byte[] buffer, bool toUpper)
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

    // public static void RunWithElevatedEH(string gamePath) =>
    //     Process.Start(new ProcessStartInfo
    //     {
    //         FileName = Environment.ProcessPath,
    //         Arguments = '\"' + gamePath + '\"',
    //         UseShellExecute = true,
    //         Verb = "runas"
    //     });
}