using Caliburn.Micro;
using ErogeHelper.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ErogeHelper.Common.Helper
{
    static class MatchProcess
    {
        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">ProcessName</param>
        /// <returns>False if not find any process</returns>
        public static bool Collect(string friendlyName)
        {
            bool newProcFind;
            // Pid mark
            List<int> procMark = new List<int>();
            // tmpProcList 每次循环 Process.GetProcessesByName() 命中的进程
            List<Process> tmpProcList = new List<Process>();
            var totalTime = new Stopwatch();
            totalTime.Start();
            do
            {
                newProcFind = false;
                DataRepository.GameProcesses.Clear();
                tmpProcList.Clear();
                #region Collect Processes To tmpProcList
                foreach (Process p in Process.GetProcessesByName(friendlyName))
                {
                    tmpProcList.Add(p);
                }
                foreach (Process p in Process.GetProcessesByName(friendlyName + ".log"))
                {
                    tmpProcList.Add(p);
                }
                foreach (Process p in Process.GetProcessesByName("main.bin"))
                {
                    tmpProcList.Add(p);
                }
                #endregion
                foreach (Process p in tmpProcList)
                {
                    DataRepository.GameProcesses.Add(p);
                    if (!procMark.Contains(p.Id))
                    {
                        procMark.Add(p.Id);
                        try
                        {
                            if (p.WaitForInputIdle(500) == false) // 500 延迟随意写的，正常启动一般在100~200范围
                            {
                                Log.Info($"Procces {p.Id} maybe stuck");
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // skip no effect exception
                            // This occurrent because process has no window event
                            Log.Info(ex.Message);
                        }

                        newProcFind = true;
                    }
                }
                // 进程找完却没有得到hWnd的可能也是存在的，所以以带hWnd的进程为主
                DataRepository.MainProcess = FindHWndProc(DataRepository.GameProcesses);

                // timeout
                if (totalTime.Elapsed.TotalSeconds > 20 && DataRepository.MainProcess == null)
                {
                    Log.Info("Timeout! Find MainWindowHandle Faied");
                    return false;
                }
            } while (newProcFind || (DataRepository.MainProcess == null));
            totalTime.Stop();

            Log.Info($"{DataRepository.GameProcesses.Count} Process(es) and window handle " +
                $"0x{Convert.ToString(DataRepository.MainProcess.MainWindowHandle.ToInt64(), 16).ToUpper()} Found. " +
                $"Spend time {totalTime.Elapsed.TotalSeconds:0.00}s");

            // Set MD5
            GameConfig.MD5 = Utils.GetFileMD5(DataRepository.MainProcess.MainModule!.FileName!);

            return true;
        }

        /// <summary>
        /// 查看一个List&lt;Process&gt;集合中是否存在MainWindowHandle
        /// </summary>
        /// <param name="procList"></param>
        /// <returns>若存在，返回其所在Process，否则返回null</returns>
        private static Process? FindHWndProc(List<Process> procList)
        {
            foreach (var p in procList)
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                    return p;
            }
            return null;
        }
    }
}
