using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IGameDataService
    {
        void InitGameMd5AndPath(string md5, string gamePath);
        string Md5 { get; }
        string GamePath { get; }

        void InitFullscreenChanged(IObservable<bool> observable);
        IObservable<bool> GameFullscreenChanged { get; }

        void SearchingProcesses(string gamePath);
        IEnumerable<Process> GameProcesses { get; }
        Process MainProcess { get; }

        HWND GameRealWindowHandle { get; }
        void SetGameRealWindowHandle(HWND handle);
    }
}

