using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using Splat;

namespace ErogeHelper.Function;

/// <summary>
/// Static global store, a kind of ambient context.
/// </summary>
internal static class State
{
    private static readonly BehaviorSubject<double> _dpiSubj = new(default);

    private static readonly BehaviorSubject<bool> _fullscreenSubj = new(default);

    public static string Md5 { get; private set; } = string.Empty;

    public static string GamePath { get; private set; } = string.Empty;

    public static string GameFolder { get; private set; } = string.Empty;

    public static bool IsUnityGame { get; private set; }

    public static nint GameRealWindowHandle { get; private set; }

    public static Process MainProcess { get; private set; } = null!;

    public static List<Process> GameProcesses { get; private set; } = null!;

    /// <summary>
    /// Indicates the screen DPI where game window is located.
    /// </summary>
    public static double Dpi => _dpiSubj.Value;

    public static bool IsFullscreen => _fullscreenSubj.Value;

    public static IObservable<bool> GameFullscreenChanged => _fullscreenSubj;

    /// <summary>
    /// True if both fullscreen and lost focus.
    /// </summary>
    //public static IObservable<bool> LostFocusChanged => _lostfocusSubj.DistinctUntilChanged();

    /// <summary>
    /// Occurs when the dpi of game's screen changed.
    /// </summary>
    public static IObservable<double> DpiChanged => _dpiSubj;

    public static nint MainGameWindowHandle { get; set; }
    public static nint TextWindowHandle { get; set; }

    #region Initialze or Update Properties Methods

    public static void Initialze(string gamePath)
    {
        GamePath = gamePath;
        LogHost.Default.Info($"Game's path: {GamePath}");

        var gameFolder = Path.GetDirectoryName(gamePath);
        ArgumentNullException.ThrowIfNull(gameFolder);
        GameFolder = gameFolder;

        try
        {
            Md5 = Utils.Md5Calculate(gamePath);
        }
        catch (IOException ex) // game with suffix ".log"
        {
            LogHost.Default.Info(ex.Message);
            Md5 = Utils.Md5Calculate(Path.Combine(
                gameFolder,
                Path.GetFileNameWithoutExtension(gamePath) + ".exe"));
        };

        IsUnityGame = File.Exists(Path.Combine(GameFolder, "UnityPlayer.dll"));
    }
    public static void InitFullscreenChanged(IObservable<bool> observable) => observable.Subscribe(_fullscreenSubj);

    public static (Process, List<Process>) InitGameProcesses((Process, List<Process>) proc) =>
        (MainProcess, GameProcesses) = (proc.Item1, proc.Item2);

    public static void UpdateGameRealWindowHandle(nint handle) => GameRealWindowHandle = handle;

    public static void UpdateDpi(double newDpi) => _dpiSubj.OnNext(newDpi);

    #endregion
}
