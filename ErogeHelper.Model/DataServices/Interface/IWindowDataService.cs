using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IWindowDataService
    {
        public void UpdateDpi(double dpi);

        /// <summary>
        /// Indicates the screen DPI where MainGameWindow is located
        /// </summary>
        public double Dpi { get; }

        /// <summary>
        /// Occurs when the dpi of game's screen changed
        /// </summary>
        public IObservable<double> DpiChanged { get; }

        void InitMainWindowHandle(HWND handle);
        HWND MainWindowHandle { get; }

        void SetTextWindowHandle(HWND handle);
        HWND? TextWindowHandle { get; }
    }
}
