using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IWindowDataService
    {
        void InitMainWindowHandle(HWND handle);
        HWND MainWindowHandle { get; }

        void SetTextWindowHandle(HWND handle);
        HWND? TextWindowHandle { get; }
    }
}
