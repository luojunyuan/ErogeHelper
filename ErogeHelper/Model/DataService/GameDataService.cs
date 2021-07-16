using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ErogeHelper.Common.Contract;
using ErogeHelper.Model.DataService.Interface;
using Splat;

namespace ErogeHelper.Model.DataService
{
    public class GameDataService : IGameDataService, IEnableLogger
    {
        public async Task LoadDataAsync(string gamePath)
        {
            GamePath = gamePath;
            GameProcesses = await ProcessCollectAsync(Path.GetFileNameWithoutExtension(gamePath))
                .ConfigureAwait(false);
            Md5 = GetFileMd5(gamePath);
            MainProcess = GameProcesses.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero) ?? new Process();
        }

        public IEnumerable<Process> GameProcesses { get; private set; } = new List<Process>();

        public string Md5 { get; private set; } = string.Empty;

        public string GamePath { get; private set; } = string.Empty;

        public Process MainProcess { get; private set; } = new();

        public bool IsMinimized { get; set; }

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        private async Task<List<Process>> ProcessCollectAsync(string friendlyName)
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();

            while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < ConstantValues.WaitGameStartTimeout)
            {
                await Task.Delay(ConstantValues.MinimumLagTime).ConfigureAwait(true);
                procList.Clear();
                procList.AddRange(Process.GetProcessesByName(friendlyName));
                procList.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
                if (!friendlyName.Equals("main.bin"))
                    procList.AddRange(Process.GetProcessesByName("main.bin"));

                mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            spendTime.Stop();

            // log 👀
            if (mainProcess is null)
            {
                this.Log().Debug("Timeout! Find MainWindowHandle Failed");
            }
            else
            {
                this.Log().Debug(
                    $"{procList.Count} Process(es) and window handle {mainProcess.MainWindowHandle:x} Found. " +
                    $"Spend time {spendTime.Elapsed.TotalSeconds:0.00}s");
            }

            return mainProcess is null ? new List<Process>() : procList;
        }

        /// <summary>
        /// Get MD5 hash by file
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <returns>Upper case string</returns>
        private static string GetFileMd5(string filePath)
        {
            FileStream file = File.OpenRead(filePath);
            var md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            foreach (var byteItem in retVal)
            {
                sb.Append(byteItem.ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }
    }
}
