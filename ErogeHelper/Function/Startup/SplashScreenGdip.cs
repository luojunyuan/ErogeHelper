using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using ErogeHelper.Function.NativeHelper;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.Function.Startup;

internal class SplashScreenGdip
{
    /// <summary>
    /// Window Class
    /// </summary>
    private const string WindowClass = "ErogeHelper Splash Image";

    /// <summary>
    /// Window Name
    /// </summary>
    private const string WindowName = "ErogeHelper Splash Image";

    /// <summary>
    /// Splash Resource Name
    /// </summary>
    private const string SplashResource = "ErogeHelper.assets.klee.png";

    /// <summary>
    /// Image Width
    /// </summary>
    private readonly int _imageWidth;

    /// <summary>
    /// Image Height
    /// </summary>
    private readonly int _imageHeight;

    /// <summary>
    /// Window Handle
    /// </summary>
    private nint _window;

    public SplashScreenGdip(int width, int height)
    {
        _imageWidth = width;
        _imageHeight = height;
    }

    public void Run()
    {
        var spalshScreenDuration = new Stopwatch();
        try
        {
            Thread.CurrentThread.Name = "Splash Screen";

            var hInstance = Process.GetCurrentProcess().Handle;

            var wndclass = new User32.WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(User32.WNDCLASSEX)),
                style = User32.WindowClassStyles.CS_DBLCLKS,
                lpfnWndProc = WindowProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = hInstance,
                hIcon = User32.LoadIcon(hInstance, User32.IDI_APPLICATION),
                hCursor = User32.LoadCursor(nint.Zero, User32.IDC_ARROW),
                hbrBackground = (HBRUSH)(int)SystemColorIndex.COLOR_WINDOW,
                lpszMenuName = null,
                lpszClassName = WindowClass,
            };

            if (User32.RegisterClassEx(wndclass) != 0)
            {
                _window = User32.CreateWindowEx(
                    User32.WindowStylesEx.WS_EX_LAYERED,
                    WindowClass,
                    WindowName,
                    User32.WindowStyles.WS_OVERLAPPED,
                    User32.CW_USEDEFAULT,
                    User32.CW_USEDEFAULT,
                    User32.CW_USEDEFAULT,
                    User32.CW_USEDEFAULT,
                    nint.Zero,
                    nint.Zero,
                    hInstance,
                    nint.Zero).DangerousGetHandle();

                if (_window != nint.Zero)
                {
                    _windowDC = User32.GetDC(new(_window)).DangerousGetHandle();

                    SetPositionAndSize();

                    User32.ShowWindow(_window, ShowWindowCommand.SW_SHOWNORMAL);

                    var startupInput = new Gdiplus.GdiplusStartupInput
                    {
                        GdiplusVersion = 1,
                        DebugEventCallback = nint.Zero,
                        SuppressBackgroundThread = false,
                        SuppressExternalCodecs = false,
                    };

                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SplashResource)!;
                    if (Gdiplus.GdiplusStartup(out _gdipToken, ref startupInput, out _) == Gdiplus.GpStatus.Ok
                        //&& Gdiplus.GdipLoadImageFromFile(_splashFile, out _splashImage) == Gdiplus.GpStatus.Ok)
                        && Gdiplus.GdipLoadImageFromStream(new GPStream(stream), out _splashImage) == Gdiplus.GpStatus.Ok)
                    {
                        spalshScreenDuration.Start();
                        DrawImage();

                        try
                        {
                            while (User32.GetMessage(out var msg, IntPtr.Zero, 0, 0) != false)
                            {
                                User32.TranslateMessage(msg);
                                User32.DispatchMessage(msg);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw new Exception("GDI+ Error, please check if image is existed");
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

        }
        finally
        {
            ReleaseAllResource();
            spalshScreenDuration.Stop();
            LogHost.Default.Info($"SplashScreen closed, spend {spalshScreenDuration.ElapsedMilliseconds}ms");
        }
    }

    public bool IsClosed { get; private set; }

    public event EventHandler? Closed;

    public void Close()
    {
        if (!IsClosed)
        {
            User32.PostMessage(_window, (uint)User32.WindowMessage.WM_CLOSE, 0, 0);
            //User32.SendMessage(_window, (uint)User32.WindowMessage.WM_CLOSE, nint.Zero, 0);
            Closed?.Invoke(this, new());

            IsClosed = true;
        }
    }

    /// <summary>
    /// Window Rectangle
    /// </summary>
    private RECT _windowRectangle;

    /// <summary>
    /// Window Size
    /// </summary>
    private SIZE _windowSize;

    /// <summary>
    /// Window DC
    /// </summary>
    private IntPtr _windowDC;

    /// <summary>
    /// Memory DC
    /// </summary>
    private nint _memoryDC = IntPtr.Zero;

    /// <summary>
    /// GDI+ Splash Image
    /// </summary>
    private nint _splashImage;

    /// <summary>
    /// GDI+ Graphics
    /// </summary>
    private nint _graphics;

    /// <summary>
    /// Memory Bitmap For SplashImage
    /// </summary>
    private nint _memoryBitmap = nint.Zero;

    /// <summary>
    /// GDI+ Token
    /// </summary>
    private nuint _gdipToken = nuint.Zero;

    /// <summary>
    /// Window Proc
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private nint WindowProc(HWND hWnd, uint msg, nint wParam, nint lParam)
    {
        try
        {
            switch ((User32.WindowMessage)msg)
            {
                case User32.WindowMessage.WM_DESTROY:
                    User32.PostQuitMessage(0);
                    return nint.Zero;
                case User32.WindowMessage.WM_DPICHANGED:
                    SetPositionAndSize(wParam.ToInt32() >> 16); //ToUInt32
                    DrawImage();
                    return nint.Zero;
                case User32.WindowMessage.WM_NCHITTEST:
                    var result = User32.DefWindowProc(hWnd, msg, wParam, lParam);
                    return result == (nint)User32.HitTestValues.HTCAPTION ? (nint)User32.HitTestValues.HTNOWHERE : result;
                default:
                    return User32.DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
        catch
        {
            //The finally in Main won't run if exception is thrown in this method.
            //This may be because this method was called by system code.
            //So we must handle exception here.
            User32.DestroyWindow(_window);
            return nint.Zero;
        }
    }

    /// <summary>
    /// Set Window Position And Size
    /// </summary>
    /// <param name="dpi"></param>
    private void SetPositionAndSize(int dpi = 0)
    {
        var screenLeft = 0;
        var screenTop = 0;
        var screenWidth = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
        var screenHeight = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);

        var monitor = User32.MonitorFromWindow(_window, User32.MonitorFlags.MONITOR_DEFAULTTONULL);
        if (monitor != HMONITOR.NULL)
        {
            var info = new User32.MONITORINFOEX();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (User32.GetMonitorInfo(monitor, ref info))
            {
                screenLeft = info.rcMonitor.left;
                screenTop = info.rcMonitor.top;
                screenWidth = info.rcMonitor.right - info.rcMonitor.left;
                screenHeight = info.rcMonitor.bottom - info.rcMonitor.top;
            }
        }

        if (dpi == 0)
        {
            var osVersion = Environment.OSVersion.Version;
            if (osVersion > new Version(10, 0, 1607))
                dpi = (int)User32.GetDpiForWindow(_window);
            else
            {
                dpi = Gdi32.GetDeviceCaps(_windowDC, Gdi32.DeviceCap.LOGPIXELSX);
            }
        }

        var windowWidth = _imageWidth * dpi / 96;
        var windowHeight = _imageHeight * dpi / 96;

        User32.SetWindowPos(
            _window,
            HWND.HWND_TOPMOST,
            (screenWidth - windowWidth) / 2 + screenLeft,
            (screenHeight - windowHeight) / 2 + screenTop,
            windowWidth, windowHeight,
            0);

        User32.GetWindowRect(_window, out _windowRectangle);

        _windowSize = new SIZE
        {
            cx = windowWidth, //_windowRectangle.right - _windowRectangle.left,
            cy = _windowRectangle.bottom - _windowRectangle.top
        };
    }

    /// <summary>
    /// Screen DC
    /// </summary>
    private readonly nint _screenDC = User32.GetDC(nint.Zero).DangerousGetHandle();

    /// <summary>
    /// Draw Image
    /// </summary>
    private void DrawImage()
    {
        if (_memoryDC != nint.Zero)
            Gdi32.DeleteDC(_memoryDC);
        if (_memoryBitmap != nint.Zero)
            Gdi32.DeleteObject(_memoryBitmap);

        _memoryDC = Gdi32.CreateCompatibleDC(_windowDC).DangerousGetHandle();
        _memoryBitmap = Gdi32.CreateCompatibleBitmap(_windowDC, _windowSize.cx, _windowSize.cy).DangerousGetHandle();
        Gdi32.SelectObject(_memoryDC, _memoryBitmap);

        if (Gdiplus.GdipCreateFromHDC(_memoryDC, out _graphics) == Gdiplus.GpStatus.Ok &&
            Gdiplus.GdipDrawImageRectI(_graphics, _splashImage, 0, 0, _windowSize.cx, _windowSize.cy) == Gdiplus.GpStatus.Ok)
        {
            var ptSrc = new POINT
            {
                X = 0,
                Y = 0,
            };
            var ptDes = new POINT
            {
                X = _windowRectangle.left,
                Y = _windowRectangle.top,
            };
            var blendFunction = new Gdi32.BLENDFUNCTION
            {
                AlphaFormat = Gdiplus.BLENDFUNCTION.AC_SRC_ALPHA,
                BlendFlags = 0,
                BlendOp = Gdiplus.BLENDFUNCTION.AC_SRC_OVER,
                SourceConstantAlpha = 255,
            };

            if (User32.UpdateLayeredWindow(
                _window,
                _screenDC,
                 ptDes,
                 _windowSize,
                 _memoryDC,
                 ptSrc,
                 0,
                 blendFunction,
                 User32.UpdateLayeredWindowFlags.ULW_ALPHA))
            {
                return;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    /// <summary>
    /// Release All Resource
    /// </summary>
    private void ReleaseAllResource()
    {
        if (_gdipToken != nuint.Zero)
            Gdiplus.GdiplusShutdown(_gdipToken);
        Gdi32.DeleteObject(_memoryBitmap);
        Gdi32.DeleteDC(_memoryDC);
        User32.ReleaseDC(_window, _windowDC);
        User32.ReleaseDC(nint.Zero, _screenDC);
    }
}
