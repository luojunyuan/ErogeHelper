using System;
using System.Collections.Generic;
using System.Diagnostics;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ITextractorService
    {
        event Action<HookParam> DataEvent;

        event Action<HookParam> SelectedDataEvent;

        TextractorSetting Setting { get; set; } 

        /// <summary>
        /// Inject hooks into processes
        /// </summary>
        /// <param name="gameProcesses"></param>
        /// <param name="setting">Textractor init callback functions depends on some parameters</param>
        void InjectProcesses(IEnumerable<Process> gameProcesses, TextractorSetting? setting = null);

        void InsertHook(string hookcode);

        void SearchRCode(string text);

        IEnumerable<string> GetConsoleOutputInfo();
    }
}