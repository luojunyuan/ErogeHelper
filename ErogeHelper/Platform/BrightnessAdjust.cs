using System.Management;
using Splat;

namespace ErogeHelper.Platform;

internal static class BrightnessAdjust
{
    private static readonly ManagementScope Scope = new("root\\WMI");
    private static readonly SelectQuery Query = new("WmiMonitorBrightness");
    private static readonly SelectQuery QueryMethods = new("WmiMonitorBrightnessMethods");

    private static readonly byte[] BrightnessLevels = GetBrightnessLevels();

    public static bool IsSupported { get; } = BrightnessLevels.Length != 0;

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

    public static void WarmUp()
    {
        if (IsSupported)
        {
            StartupBrightness(GetBrightness());
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
        foreach (ManagementBaseObject obj in objCollection)
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
        iPercent = iPercent switch
        {
            < 0 => 0,
            > 100 => 100,
            _ => iPercent
        };

        // iPercent is in the range of brightnessLevels
        if (iPercent > BrightnessLevels[^1]) 
            return;
        
        // Default level 100
        byte level = 100;
        foreach (var item in BrightnessLevels)
        {
            // Found most close one of iPercent in brightnessLevels
            if (item < iPercent) 
                continue;
            
            level = item;
            break;
        }
        SetBrightness(level);
    }

    /// <summary>
    /// Set the brightness level to the targetBrightness
    /// </summary>
    private static void SetBrightness(byte targetBrightness)
    {
        using ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, QueryMethods);
        using ManagementObjectCollection objectCollection = searcher.Get();
        foreach (var o in objectCollection)
        {
            var mObj = (ManagementObject)o;
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
            foreach (var managementBaseObject in moc)
            {
                var o = (ManagementObject)managementBaseObject;
                bLevels = (byte[])o.GetPropertyValue("Level");
                // Only work on the first object
                break;
            }
        }
        catch (Exception ex)
        {
            LogHost.Default.Debug("System.Management.ManagementException " + ex.Message);
        }

        return bLevels;
    }
}
