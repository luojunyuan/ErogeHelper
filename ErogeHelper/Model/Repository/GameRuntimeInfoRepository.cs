using ErogeHelper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ErogeHelper.Model.Repository
{
    public class GameRuntimeInfoRepository
    {
        private string _md5 = string.Empty;
        private IEnumerable<Process> _gameProcesses = new List<Process>();
        private Process _mainProcess = new();
        private string _gamePath = string.Empty;

        public (string, Process) Init(IEnumerable<Process> gameProcesses)
        {
            _gameProcesses = gameProcesses;

            _mainProcess = GameProcesses.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero) ??
                           throw new InvalidOperationException();
            _gamePath = MainProcess.MainModule?.FileName ?? string.Empty;
            _md5 = Utils.GetFileMd5(GamePath);

            return (Md5, MainProcess);
        }

        public string Md5 => _md5;

        public IEnumerable<Process> GameProcesses => _gameProcesses;

        public Process MainProcess => _mainProcess;

        /// <summary>
        /// For helping find game information, with <see cref="Utils.GetGameNamesByPath"/>
        /// </summary>
        public string GamePath => _gamePath;
    }
}