using System;
using System.Drawing;
using System.Linq;
using ErogeHelper.Common;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class AdjustScreenBrightness : IAdjustScreenBrightness
    {
        public bool GetBrightness(out short currentBrightness, out short minBrightness, out short maxBrightness)
        {
            var handle = Graphics.FromHwnd(IntPtr.Zero).GetHdc();

            //0-50 亮度变化太小，所以从50开始
            minBrightness = 50;
            maxBrightness = 100;
            var ramp = default(NativeMethods.Ramp);
            var deviceGammaRamp = NativeMethods.GetDeviceGammaRamp(handle, ref ramp);
            currentBrightness = (short)((deviceGammaRamp ? CalAllGammaVal(ramp) : 0.5) * 100);

            return deviceGammaRamp;
        }

        public void SetBrightness(short brightness)
        {
            var handle = Graphics.FromHwnd(IntPtr.Zero).GetHdc();
            var value = (double)brightness / 100;
            NativeMethods.Ramp ramp = default;
            ramp.Red = new ushort[256];
            ramp.Green = new ushort[256];
            ramp.Blue = new ushort[256];

            for (var i = 1; i < 256; i++)
            {
                var tmp = (ushort)(i * 255 * value);
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, tmp));
            }

            _ = NativeMethods.SetDeviceGammaRamp(handle, ref ramp);
        }

        private static double CalAllGammaVal(NativeMethods.Ramp ramp)
        {
            return Math.Round(((CalColorGammaVal(ramp.Blue) + CalColorGammaVal(ramp.Red) +
                                CalColorGammaVal(ramp.Green)) / 3), 2);
        }

        private static double CalColorGammaVal(ushort[] line)
        {
            var max = line.Max();
            var min = line[0];
            var index = Array.FindIndex(line, n => n == max);
            var gamma = Math.Round((((double)(max - min) / index) / 255), 2);
            return gamma;
        }
    }
}