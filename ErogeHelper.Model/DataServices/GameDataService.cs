using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices
{
    public class GameDataService : IGameDataService, IEnableLogger
    {
        public void InitGameMd5AndPath(string md5, string gamePath) => (Md5, GamePath) = (md5, gamePath);
        public string Md5 { get; private set; } = string.Empty;
        public string GamePath { get; private set; } = string.Empty;

        public void InitFullscreenChanged(IObservable<bool> observable) => GameFullscreenChanged = observable;
        public IObservable<bool> GameFullscreenChanged { get; private set; } = null!;
        
        public void SearchingProcesses(string gamePath) =>
            (MainProcess, GameProcesses) = ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
        public IEnumerable<Process> GameProcesses { get; private set; } = null!;
        public Process MainProcess { get; private set; } = null!;

        public HWND GameRealWindowHandle { get; private set; }
        public void SetGameRealWindowHandle(HWND handle) => GameRealWindowHandle = handle;


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

            while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < ConstantValue.WaitGameStartTimeout)
            {
                Thread.Sleep(ConstantValue.UIMinimumResponseTime);
                procList = Utils.GetProcessesByFriendlyName(friendlyName);

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
    }
}

