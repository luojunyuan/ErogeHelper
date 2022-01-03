using System.Reactive.Subjects;

namespace ErogeHelper.Shared;

public static class AmbiantContext
{
    /// <summary>
    /// Indicates the screen DPI where MainGameWindow is located
    /// </summary>
    public static double Dpi { get; private set; } = 1;

    /// <summary>
    /// Occurs when the dpi of game's screen changed
    /// </summary>
    public static IObservable<double> DpiChanged => _dpiSubj;
    
    private static readonly Subject<double> _dpiSubj = new();

    public static void UpdateDpi(double newDpi)
    {
        Dpi = newDpi;
        _dpiSubj.OnNext(newDpi);
    }
}
