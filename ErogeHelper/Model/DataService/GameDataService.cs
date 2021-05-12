using ErogeHelper.Common.Contract;
using ErogeHelper.Model.DataService.Interface;
using Microsoft.Windows.Sdk;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.DataService
{
    public class GameDataService : IGameDataService, IEnableLogger
    {
        public void LoadData(string gamePath)
        {
            _gamePath = gamePath;
            _gameProcesses = ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
            _md5 = GetFileMd5(gamePath);
            _mainProcess = GameProcesses.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero) ?? new();
        }

        public IEnumerable<Process> GameProcesses => _gameProcesses;

        public string Md5 => _md5;

        public string GamePath => _gamePath;

        public Process MainProcess => _mainProcess;

        #region Private Fields

        private IEnumerable<Process> _gameProcesses = new List<Process>();

        private string _md5 = string.Empty;

        private string _gamePath = string.Empty;

        private Process _mainProcess = new();

        #endregion

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        private List<Process> ProcessCollect(string friendlyName)
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();
            while (mainProcess is null && spendTime.Elapsed.TotalMilliseconds < ConstantValues.WaitGameStartTimeout)
            {
                procList.Clear();
                procList.AddRange(Process.GetProcessesByName(friendlyName));
                procList.AddRange(Process.GetProcessesByName(friendlyName + ".log"));

                // 进程找完却没有得到hWnd的可能也是存在的，所以以带hWnd的进程为主
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
                this.Log().Debug($"{procList.Count} Process(es) and window handle " +
                         $"0x{Convert.ToString(mainProcess.MainWindowHandle.ToInt64(), 16).ToUpper()} Found. " +
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
