using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        public static BitmapImage PeIcon2BitmapImage(string fullPath)
        {
            BitmapImage result = new ();
            Stream stream = new MemoryStream();

            var iconBitmap = Icon.ExtractAssociatedIcon(fullPath)!.ToBitmap();
            iconBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            iconBitmap.Dispose();
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName"><see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        public static (IEnumerable<Process>, Process) ProcessCollect(string friendlyName)
        {
            const int timeoutSeconds = 20;
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();
            while (mainProcess is null && spendTime.Elapsed.TotalSeconds < timeoutSeconds)
            {
                procList.AddRange(Process.GetProcessesByName(friendlyName));
                procList.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
                procList.AddRange(Process.GetProcessesByName("main.bin"));

                // 进程找完却没有得到hWnd的可能也是存在的，所以以带hWnd的进程为主
                mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            spendTime.Stop();

            // log 👀
            if (mainProcess is null)
            {
                Log.Info("Timeout! Find MainWindowHandle Failed");
            }
            else
            {
                Log.Info($"{procList.Count} Process(es) and window handle " +
                         $"0x{Convert.ToString(mainProcess.MainWindowHandle.ToInt64(), 16).ToUpper()} Found. " +
                         $"Spend time {spendTime.Elapsed.TotalSeconds:0.00}s");
            }

            return mainProcess is null ? (new List<Process>(), new Process()) : (procList, mainProcess);
        }

        public static void HideWindowInAltTab(Window window)
        {
            var windowInterop = new WindowInteropHelper(window);
            var exStyle = NativeMethods.GetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE);
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE, exStyle);
        }
    }
}