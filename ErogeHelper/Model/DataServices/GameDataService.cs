using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices
{
    public class GameDataService : IGameDataService, IEnableLogger
    {
        public HWND GameRealWindowHandle { get; private set; }

        public void Init(string md5, string gamePath) => (Md5, GamePath) = (md5, gamePath);

        public string Md5 { get; private set; } = string.Empty;

        public string GamePath { get; private set; } = string.Empty;

        public IEnumerable<Process> GameProcesses { get; private set; } = null!;

        public Process MainProcess { get; private set; } = null!;

        public void SearchingProcesses(string gamePath) =>
            (MainProcess, GameProcesses) = ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        private static (Process, List<Process>) ProcessCollect(string friendlyName)
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();

            while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < ConstantValues.WaitGameStartTimeout)
            {
                Thread.Sleep(ConstantValues.MinimumLagTime);
                procList = Utils.GetProcessesByfriendlyName(friendlyName);

                mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            spendTime.Stop();

            if (mainProcess is null)
            {
                throw new TimeoutException("Timeout! Find MainWindowHandle Failed");
            }

            LogHost.Default.Debug(
                $"{procList.Count} Process(es) and MainWindowHandle 0x{mainProcess.MainWindowHandle:X8} Found.");

            return (mainProcess, procList);
        }

        public void SetGameRealWindowHandle(HWND handle) => GameRealWindowHandle = handle;
    }
}
