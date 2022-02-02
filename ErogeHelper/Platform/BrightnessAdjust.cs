using System.Management;
using Splat;

namespace ErogeHelper.Platform;

class BrightnessAdjust
{
    private readonly static ManagementScope Scope = new("root\\WMI");
    private readonly static SelectQuery Query = new("WmiMonitorBrightness");
    private readonly static SelectQuery QueryMethods = new("WmiMonitorBrightnessMethods");

    private readonly static byte[] _brightnessLevels = GetBrightnessLevels();

    public static bool IsSupported { get; } = _brightnessLevels.Length != 0;

    public static void IncreaseBrightness()
    {
        if (IsSupported)
        {
            StartupBrightness(GetBrightness() + 10);
        }
    }

    public static void DecreaseBrightness()
    {
        if (IsSupported)
        {
            StartupBrightness(GetBrightness() - 10);
        }
    }

    /// <summary>
    /// Returns the current brightness setting
    /// </summary>
    private static int GetBrightness()
    {
        using ManagementObjectSearcher searcher = new(Scope, Query);
        using ManagementObjectCollection objCollection = searcher.Get();

        byte curBrightness = 0;
        foreach (ManagementObject obj in objCollection)
        {
            curBrightness = (byte)obj.GetPropertyValue("CurrentBrightness");
            break;
        }

        return curBrightness;
    }

    /// <summary>
    /// Convert the brightness percentage to a byte and set the brightness using SetBrightness()
    /// </summary>
    private static void StartupBrightness(int iPercent)
    {
        if (iPercent < 0)
        {
            iPercent = 0;
        }
        else if (iPercent > 100)
        {
            iPercent = 100;
        }

        // iPercent is in the range of brightnessLevels
        if (iPercent >= 0 && iPercent <= _brightnessLevels[^1])
        {
            // Default level 100
            byte level = 100;
            foreach (byte item in _brightnessLevels)
            {
                // Found most close one of iPercent in brightnessLevels
                if (item >= iPercent)
                {
                    level = item;
                    break;
                }
            }
            SetBrightness(level);
        }
    }

    /// <summary>
    /// Set the brightnesslevel to the targetBrightness
    /// </summary>
    private static void SetBrightness(byte targetBrightness)
    {
        using ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, QueryMethods);
        using ManagementObjectCollection objectCollection = searcher.Get();
        foreach (ManagementObject mObj in objectCollection)
        {
            // Note the reversed order - won't work otherwise!
            mObj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, targetBrightness });
            // Only work on the first object
            break;
        }
    }

    private static byte[] GetBrightnessLevels()
    {
        // Output current brightness
        using ManagementObjectSearcher mos = new(Scope, Query);

        byte[] bLevels = Array.Empty<byte>();
        try
        {
            using ManagementObjectCollection moc = mos.Get();
            foreach (ManagementObject o in moc)
            {
                bLevels = (byte[])o.GetPropertyValue("Level");
                // Only work on the first object
                break;
            }
        }
        catch (Exception ex)
        {
            LogHost.Default.Debug(ex.Message);
        }

        return bLevels;
    }
}
