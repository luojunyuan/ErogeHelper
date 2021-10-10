using System.Collections.Generic;
using System.Diagnostics;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IGameDataService
    {
        string Md5 { get; }

        string GamePath { get; }

        HWND GameRealWindowHandle { get; }

        IEnumerable<Process> GameProcesses { get; }

        Process MainProcess { get; }

        void Init(string md5, string gamePath);

        void SearchingProcesses(string gamePath);

        void SetGameRealWindowHandle(HWND handle);
    }
}
