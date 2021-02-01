using System;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ErogeHelper.Common.Helper
{
    class AdjustScreenBuilder
    {
        /// <summary>
        /// 创建一个调整屏幕亮度的对象
        /// </summary>
        /// <returns>如果返回空，则代表此屏幕不支持</returns>
        public static IAdjustScreen? CreateAdjustScreen(IntPtr handle)
        {
            short min = 0;
            short current = 0;
            short max = 0;
            var adjustScreenByGdi32 = new AdjustScreenByGdi32();
            var adjustScreenByGdi32Value = adjustScreenByGdi32.GetBrightness(handle, ref min, ref current, ref max);
            if (adjustScreenByGdi32Value)
                return adjustScreenByGdi32;

            return null;
        }
    }

    public class AdjustScreenByGdi32 : IAdjustScreen
    {
        [DllImport("gdi32.dll")]
        public static extern bool GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        private double CalAllGammaVal(RAMP ramp)
        {
            return Math.Round(((CalColorGammaVal(ramp.Blue) + CalColorGammaVal(ramp.Red) +
                                CalColorGammaVal(ramp.Green)) / 3), 2);
        }

        /// <summary>
        /// internal类型不要修改，不希望外界构造。可通过<see cref="AdjustScreenBuilder.CreateAdjustScreen"/>创建
        /// 由于调整屏幕亮度有多种方案，不同的屏幕适配不同的方案。所以不需要外界做过多判断。
        /// </summary>
        internal AdjustScreenByGdi32()
        {
        }

        private double CalColorGammaVal(ushort[] line)
        {
            var max = line.Max();
            var min = line[0];
            var index = Array.FindIndex(line, n => n == max);
            var gamma = Math.Round((((double)(max - min) / index) / 255), 2);
            return gamma;
        }

        /// <summary>
        /// 读取屏幕亮度
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="minBrightness"></param>
        /// <param name="currentBrightness"></param>
        /// <param name="maxBrightness"></param>
        /// <returns></returns>
        public bool GetBrightness(IntPtr handle, ref short minBrightness, ref short currentBrightness,
            ref short maxBrightness)
        {
            handle = Graphics.FromHwnd(IntPtr.Zero).GetHdc();
            //0-50 亮度变化太小，所以从50开始
            minBrightness = 50;
            maxBrightness = 100;
            var ramp = default(RAMP);
            var deviceGammaRamp = GetDeviceGammaRamp(handle, ref ramp);
            currentBrightness = (short)((deviceGammaRamp ? CalAllGammaVal(ramp) : 0.5) * 100);
            return deviceGammaRamp;
        }

        /// <summary>
        /// 设置屏幕亮度
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public bool SetBrightness(IntPtr handle, short brightness)
        {
            handle = Graphics.FromHwnd(IntPtr.Zero).GetHdc();
            double value = (double)brightness / 100;
            RAMP ramp = default(RAMP);
            ramp.Red = new ushort[256];
            ramp.Green = new ushort[256];
            ramp.Blue = new ushort[256];

            for (int i = 1; i < 256; i++)
            {
                var tmp = (ushort)(i * 255 * value);
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, tmp));
            }

            var deviceGammaRamp = SetDeviceGammaRamp(handle, ref ramp);
            return deviceGammaRamp;
        }
    }

    public interface IAdjustScreen
    {
        bool GetBrightness(IntPtr handle, ref short minBrightness,
            ref short currentBrightness,
            ref short maxBrightness);

        bool SetBrightness(IntPtr handle, short brightness);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RAMP
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Red;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Green;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Blue;
    }
}
