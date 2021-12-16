using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices;

public class WindowDataService : IWindowDataService
{
    public void InitMainWindowHandle(HWND handle) => MainWindowHandle = handle;

    public HWND MainWindowHandle { get; private set; }

    public void InitTextWindowHandle(HWND handle) => TextWindowHandle = handle;

    public HWND TextWindowHandle { get; private set; }

    public double Dpi { get; set; }

    public IObservable<double> DpiChanged => _dpiSubj;

    public void DpiOnNext(double dpi) => _dpiSubj.OnNext(dpi);

    private readonly ReplaySubject<double> _dpiSubj = new(1);
}
