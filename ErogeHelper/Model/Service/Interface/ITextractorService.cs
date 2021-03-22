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

        void InjectProcesses();

        void InsertHook(string hookcode);

        void SearchRCode(string text);
    }
}