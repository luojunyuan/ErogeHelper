using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.UI.Xaml;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ErogeHelper.ProcessSelector.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private IntPtr _hwnd;

        public MainWindow()
        {
            InitializeComponent();
            InitializeProperties();
            SubClassingWin32();
        }

        private void InitializeProperties()
        {
            _hwnd = this.GetWindowHandle();
            Title = "Process Selector";
            this.CenterOnScreen(_width, _height);
            HwndExtensions.SetWindowStyle(
                _hwnd,
                WindowStyle.Visible | WindowStyle.Caption | WindowStyle.SysMenu);
            DpiChanged += MainWindow_DpiChanged;
        }

        private double _width = 400;
        private double _height = 250;

        private void MainWindow_DpiChanged(object? sender, WindowDpiChangedEventArgs e)
        {
            var currentDpi = this.GetDpiForWindow() / 100.0;
            _width = _width / currentDpi * e.Dpi;
            _height = _height / currentDpi * e.Dpi;

            //this.SetWindowSize(_width, _height);
        }

        private void InjectButtonOnClick(object sender, RoutedEventArgs e)
        {
            InjectButton.Width = 10;
        }

        private void ProcessComboBoxOnDropDownOpened(object sender, object e)
        {

        }

        private void ProcessComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SetWindowSizeWin32(IntPtr hwnd, int width, int height)
        {
            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }
        //private Orientation GetWindowOrientationWin32(IntPtr hwnd)
        //{
        //    Orientation orientationEnum;
        //    int theScreenWidth = DisplayInformation.GetDisplay(hwnd).ScreenWidth;
        //    int theScreenHeight = DisplayInformation.GetDisplay(hwnd).ScreenHeight;
        //    if (theScreenWidth > theScreenHeight)
        //        orientationEnum = Orientation.Landscape;
        //    else
        //        orientationEnum = Orientation.Portrait;
        //    return orientationEnum;
        //}
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        private void SubClassingWin32()
        {
            //Get the Window's HWND
            if (_hwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("The Window Handle is null.");

            }
            newWndProc = new WinProc(NewWindowProc);
            oldWndProc = SetWindowLongPtr(_hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }
        private delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        private WinProc? newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, newProc);
            else
                return SetWindowLongPtr32(hWnd, nIndex, newProc);
        }
        private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_DPICHANGED:
                    if (DpiChanged is not null)
                    {
                        uint dpi = HiWord(wParam);
                        OnWindowDpiChanged((int)dpi);
                    }
                    break;
                case PInvoke.User32.WindowMessage.WM_DISPLAYCHANGE:
                    //if (this.OrientationChanged is not null)
                    //{
                    //    var newOrinetation = GetWindowOrientationWin32(hWnd);
                    //    if (newOrinetation != _currentOrientation)
                    //    {
                    //        _currentOrientation = newOrinetation;
                    //        OnWindowOrientationChanged(newOrinetation);
                    //    }
                    //}
                    break;
            }
            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        public event EventHandler<WindowDpiChangedEventArgs>? DpiChanged;
        private void OnWindowDpiChanged(int newDpi)
        {
            WindowDpiChangedEventArgs windowDpiChangedEvent = new(this, newDpi / 100.0);
            DpiChanged?.Invoke(this, windowDpiChangedEvent);
        }
        public class WindowDpiChangedEventArgs : EventArgs
        {
            public Window Window { get; private set; }
            public double Dpi { get; private set; }

            public WindowDpiChangedEventArgs(Window window, double newDpi)
            {
                Window = window;
                Dpi = newDpi;
            }
        }
        private static uint HiWord(IntPtr ptr)
        {
            uint value = (uint)(int)ptr;
            if ((value & 0x80000000) == 0x80000000)
                return (value >> 16);
            else
                return (value >> 16) & 0xffff;
        }
    }
}
