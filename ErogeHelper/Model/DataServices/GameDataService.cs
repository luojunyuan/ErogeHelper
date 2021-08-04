using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ErogeHelper.Model.DataServices
{
    public class GameDataService : IGameDataService, IEnableLogger
    {
        public IEnumerable<Process> GameProcesses { get; private set; } = new List<Process>();

        public string Md5 { get; private set; } = string.Empty;

        public string GamePath { get; private set; } = string.Empty;

        public Process MainProcess { get; private set; } = new();

        public void LoadData(string gamePath)
        {
            GamePath = gamePath;
            (MainProcess, GameProcesses) = ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
            Md5 = Utils.Md5Calculate(File.ReadAllBytes(GamePath));
        }

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        private (Process, List<Process>) ProcessCollect(string friendlyName)
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();

            while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < ConstantValues.WaitGameStartTimeout)
            {
                Thread.Sleep(ConstantValues.MinimumLagTime);
                procList.Clear();
                procList.AddRange(Process.GetProcessesByName(friendlyName));
                procList.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
                if (!friendlyName.Equals("main.bin", StringComparison.Ordinal))
                {
                    procList.AddRange(Process.GetProcessesByName("main.bin"));
                }

                mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            spendTime.Stop();

            // log 👀
            if (mainProcess is null)
            {
                this.Log().Debug("Timeout! Find MainWindowHandle Failed");
                throw new ArgumentNullException(nameof(mainProcess), "Find MainWindowHandle Failed");
            }
            else
            {
                this.Log().Debug(
                    $"{procList.Count} Process(es) and window handle {mainProcess.MainWindowHandle:x} Found. " +
                    $"Spend time {spendTime.Elapsed.TotalSeconds:0.00}s");
            }

            return (mainProcess, procList);
        }
    }
}
