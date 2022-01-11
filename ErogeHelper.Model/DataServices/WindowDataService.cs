using ErogeHelper.Model.DataServices.Interface;
using Vanara.PInvoke;

namespace ErogeHelper.Shared;

public class WindowDataService : IWindowDataService
{
    public void InitMainWindowHandle(HWND handle) => MainWindowHandle = handle;
    public HWND MainWindowHandle { get; private set; }

    public void SetTextWindowHandle(HWND handle) => TextWindowHandle = handle;
    public HWND? TextWindowHandle { get; private set; }
}
