using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfExtend;
using Karna.Magnification;
using Splat;

namespace ErogeHelper.View.Magnifier
{
    internal class MagWindow : Window, IEnableLogger
    {
        private readonly float magnification;
        private static readonly double StableScreenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        private static readonly double StableScreenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

        private readonly int _sourceWidth;
        private readonly int _sourceHeight;
        private readonly RECT _sourceRect = new RECT();
        private readonly bool _smoothing;

        public MagWindow(int left, int top, int sourceWidth, int sourceHeight, bool smoothing)
        {
            _sourceRect.left = left;
            _sourceRect.top = top;
            _sourceRect.right = left + _sourceWidth;
            _sourceRect.bottom = top + _sourceHeight;
            _smoothing = smoothing;

            _sourceWidth = sourceWidth;
            _sourceHeight = sourceHeight;

            // DPI に関わる
            var screenWidth = StableScreenWidth / State.Dpi;
            var screenHeight = StableScreenHeight / State.Dpi;

            magnification = (float)screenWidth / _sourceWidth;
            LogHost.Default.Debug($"Scale x{magnification}");

            Left = 0;
            Width = screenWidth;
            Height = _sourceHeight * magnification;
            Top = screenHeight - Height;

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent;
            _handle = WpfHelper.GetWpfWindowHandle(this);
            HwndTools.HideWindowInAltTab(_handle);
            ShowInTaskbar = false;

            Loaded += MagWindowOnLoaded;
            Closed += MagWindowOnClosed;
        }


        private RECT _magWindowRect = new RECT();
        private nint _handle;
        private DispatcherTimer _timer = new();
        private nint _hwndMag;

        private void MagWindowOnLoaded(object? sender, RoutedEventArgs e)
        {
            // 順番　initialize -> Setuo -> Start timer
            var initialized = NativeMethods.MagInitialize();
            if (initialized)
            {
                // Just like a fill parameter
                var hInst = NativeMethods.GetModuleHandle(null);

                // Create a magnifier control that fills the client area.
                NativeMethods.GetClientRect(_handle, ref _magWindowRect); // _magWindowRect get the window height width

                _hwndMag = NativeMethods.CreateWindow(
                    (int)ExtendedWindowStyles.WS_EX_CLIENTEDGE,
                NativeMethods.WC_MAGNIFIER,
                "MagnifierWindow",
                (int)WindowStyles.WS_CHILD | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR | (int)WindowStyles.WS_VISIBLE,
                    _magWindowRect.left, _magWindowRect.top, _magWindowRect.right, _magWindowRect.bottom,
                    _handle, // this parameter bind to wpf window
                    IntPtr.Zero, hInst, IntPtr.Zero);

                if (_hwndMag == IntPtr.Zero)
                {
                    throw new ArgumentNullException();
                }

                if (_smoothing)
                {
                    var isSmoothingActive = NativeMethods.MagSetLensUseBitmapSmoothing(_hwndMag, true);
                }

                // Set the magnification factor.
                Transformation matrix = new Transformation(magnification);
                NativeMethods.MagSetWindowTransform(_hwndMag, ref matrix);

                _timer.Tick += UpdateMaginifier;
                _timer.Interval = TimeSpan.FromMilliseconds(NativeMethods.USER_TIMER_MINIMUM);
                _timer.Start();
            }
        }


        private void UpdateMaginifier(object? sender, EventArgs e)
        {
            // Set the source rectangle for the magnifier control.
            NativeMethods.MagSetWindowSource(_hwndMag, _sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            NativeMethods.SetWindowPos(
                _handle,
                NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);

            // Force redraw.
            NativeMethods.InvalidateRect(_hwndMag, IntPtr.Zero, true);
        }

        private void MagWindowOnClosed(object? sender, EventArgs e)
        {
            _timer.Stop();
            NativeMethods.MagUninitialize();
        }
    }
}
