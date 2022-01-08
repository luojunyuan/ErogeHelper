using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using Vanara.PInvoke;

namespace ErogeHelper.Shared;

public class WindowDataService : IWindowDataService
{
    /// <inheritdoc />
    public double Dpi { get; private set; }

    /// <inheritdoc />
    public IObservable<double> DpiChanged => _dpiSubj;

    private readonly Subject<double> _dpiSubj = new();

    public void UpdateDpi(double newDpi)
    {
        Dpi = newDpi;
        _dpiSubj.OnNext(newDpi);
    }

    public void InitMainWindowHandle(HWND handle) => MainWindowHandle = handle;
    public HWND MainWindowHandle { get; private set; }

    public void SetTextWindowHandle(HWND handle) => TextWindowHandle = handle;
    public HWND? TextWindowHandle { get; private set; }
}
