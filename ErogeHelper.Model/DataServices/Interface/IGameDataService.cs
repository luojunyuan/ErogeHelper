using System.Diagnostics;
using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface;

public interface IGameDataService : IDisposable
{
    void InitGameMd5AndPath(string md5, string gamePath);
    string Md5 { get; }
    string GamePath { get; }

    void InitFullscreenChanged(IObservable<bool> observable);
    IObservable<bool> GameFullscreenChanged { get; }

    /// <summary>
    /// Initialize game process by searching it
    /// </summary>
    /// <exception cref="TimeoutException"></exception>
    void InitGameProcesses(string gamePath);
    List<Process> GameProcesses { get; }
    Process MainProcess { get; }

    HWND GameRealWindowHandle { get; }
    void SetGameRealWindowHandle(HWND handle);
}

