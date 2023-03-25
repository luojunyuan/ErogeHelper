using ErogeHelper.AssistiveTouch.NativeMethods;

namespace ErogeHelper.AssistiveTouch.Core
{
    internal class Fullscreen
    {
        public static bool GameInFullscreen { get; private set; }

        public static event EventHandler<bool>? FullscreenChanged;

        public static void UpdateFullscreenStatus()
        {
            var isFullscreen = IsWindowFullscreen(App.GameWindowHandle);
            if (GameInFullscreen != isFullscreen)
                FullscreenChanged?.Invoke(null, isFullscreen);
            GameInFullscreen = isFullscreen;
        }

        // See: http://www.msghelp.net/showthread.php?tid=67047&pid=740345
        public static bool IsWindowFullscreen(IntPtr hwnd)
        {
            User32.GetWindowRect(hwnd, out var rect);
            return rect.left < 50 && rect.top < 50 &&
                rect.Width >= User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN) &&
                rect.Height >= User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);
        }
    }
}
