using System;
using System.Collections.Generic;
using System.Diagnostics;
using ErogeHelper.Common.Entity;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ITextractorService
    {
        event Action<HookParam> DataEvent;

        event Action<HookParam> SelectedDataEvent;

        bool HasInjected { get; set; }

        void InjectProcesses(IEnumerable<Process> processEnumerable);

        void InsertHook(string hookcode);

        void SearchRCode(string text);

        void UpdateSelectedThreadSetting(HookParam setting);
    }
}