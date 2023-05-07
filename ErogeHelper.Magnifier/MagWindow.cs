using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace ErogeHelper.Magnifier
{
    internal class MagWindow
    {
        private static readonly double StableScreenWidth = GetSystemMetrics(SM_CXSCREEN);
        private static readonly double StableScreenHeight = GetSystemMetrics(SM_CYSCREEN);
        private readonly WindowProc StaticWndProcDelegate;

        private readonly Timer MagTimer = new Timer();

        private readonly float Magnification;
        private readonly IntPtr Host;
        private readonly IntPtr MagControl;

        private RECT _sourceRect;

        public MagWindow(int left, int top, int sourceWidth, int sourceHeight, int timerCmd)
        {
            Magnification = (float)StableScreenWidth / sourceWidth;
            _sourceRect = new RECT(left, top, 0, 0);

            const string WindowClass = "ErogeHelper Magnifier Class";
            var hInstance = Process.GetCurrentProcess().Handle;
            StaticWndProcDelegate = HostWndProc;
            var wndClass = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                lpfnWndProc = StaticWndProcDelegate,
                hInstance = hInstance,
                lpszClassName = WindowClass,
            };
            RegisterClassEx(wndClass);

            // host size depend on height, height depend on source width and height and destination width
            var height = (int)(StableScreenHeight - sourceHeight * Magnification);
            var hostRect = new RECT(0, height, (int)StableScreenWidth, (int)StableScreenHeight);
            Host = CreateWindowEx(
                WindowStylesEx.WS_EX_LAYERED | WindowStylesEx.WS_EX_TRANSPARENT,
                WindowClass,
                "ErogeHelper.Magnifier", // title
                WindowStyles.WS_POPUP,
                hostRect.Left, hostRect.Top, hostRect.Width, hostRect.Height,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);

            const uint LWA_ALPHA = 0x00000002;
            SetLayeredWindowAttributes(Host, default, 255, LWA_ALPHA);

            const uint SWP_SHOWWINDOW = 0x0001;
            ShowWindow(Host, SWP_SHOWWINDOW);

            // 順番　initialize -> set uo -> start timer
            var initialized = MagInitialize();
            if (initialized)
            {
                // Create a magnifier control that fills the client area.
                GetClientRect(Host, out var _magWindowRect); // _magWindowRect get the window height width

                const int WS_EX_CLIENTEDGE = 0x00000200;
                const string WC_MAGNIFIER = "Magnifier";
                MagControl = CreateWindow(WS_EX_CLIENTEDGE,
                    WC_MAGNIFIER,
                    "MagnifierWindow", // title?
                    (int)WindowStyles.WS_CHILD | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR | (int)WindowStyles.WS_VISIBLE,
                    // 0, 0, n, m
                    _magWindowRect.Left, _magWindowRect.Top, _magWindowRect.Right, _magWindowRect.Bottom,
                    Host, // this parameter bind to wpf window
                    IntPtr.Zero, hInstance, IntPtr.Zero);

                if (MagControl == IntPtr.Zero)
                    throw new ArgumentNullException();

                // if size or proportional changed ->  MagSetWindowTransform
                // if left top changed -> MagSetWindowSource
                MagSetWindowSource(MagControl, new RECT(_sourceRect.Left, _sourceRect.Top, 0, 0));

                MagSetLensUseBitmapSmoothing(MagControl, true);

                // Set the magnification factor.
                var matrix = new Transformation(Magnification);
                MagSetWindowTransform(MagControl, ref matrix);

                const double USER_TIMER_MINIMUM = 0xA;
                // const double FPS60 = 1000 / 60;
                const double FPS24 = 1000 / 24;
                //const double Slow = 120; // 8 fps can be more good Pending to test 
                MagTimer.Elapsed += UpdateMaginifier;
                MagTimer.Interval = timerCmd == 0 ? USER_TIMER_MINIMUM : FPS24;
                MagTimer.Start();
            }
        }

        public void Run()
        {
            while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(msg);
                DispatchMessage(msg);
            }

            MagTimer.Stop();
            MagUninitialize();
        }

        public void UpdatePosition(int leftDelta, int topDelta)
        {
            _sourceRect.Left += leftDelta;
            _sourceRect.Top += topDelta;
            // Set the source rectangle for the magnifier control.
            MagSetWindowSource(MagControl, _sourceRect);
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 0x02;
        private const int SWP_NOSIZE = 0x01;
        private void UpdateMaginifier(object sender, EventArgs e)
        {
            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            SetWindowPos(Host, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);

            // Force redraw.
            InvalidateRect(MagControl, IntPtr.Zero, true);
        }

        private const uint WM_DESTROY = 0x0002;
        private const uint WM_SIZE = 0x0005;

        private IntPtr HostWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_DESTROY:
                    PostQuitMessage();
                    return IntPtr.Zero;

                case WM_SIZE:
                    //    if (_magControl != IntPtr.Zero)
                    //    {
                    //        GetClientRect(_host, out var magWindowRect);
                    //        // Resize the control to fill the window.
                    //        SetWindowPos(_magControl, IntPtr.Zero, magWindowRect.Left, magWindowRect.Top, magWindowRect.Right, magWindowRect.Bottom, 0);
                    //    }
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Transformation
        {
            public float m00;
            public float m10;
            public float m20;
            public float m01;
            public float m11;
            public float m21;
            public float m02;
            public float m12;
            public float m22;

            public Transformation(float magnificationFactor)
                : this()
            {
                m00 = magnificationFactor;
                m11 = magnificationFactor;
                m22 = 1.0f;
            }
        }

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagInitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagUninitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetWindowTransform(IntPtr hwnd, ref Transformation pTransform);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetWindowSource(IntPtr hwnd, RECT rect);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool MagSetLensUseBitmapSmoothing(IntPtr hwnd, bool isSmoothing);

        internal enum MagnifierStyle : int
        {
            MS_SHOWMAGNIFIEDCURSOR = 0x0001,
            MS_CLIPAROUNDCURSOR = 0x0002,
            MS_INVERTCOLORS = 0x0004
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, [MarshalAs(UnmanagedType.Bool)] bool erase);



        [DllImport("user32.dll")]
        public static extern bool GetMessage(out IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(in IntPtr lpMsg);
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(in IntPtr lpMsg);


        [DllImport("user32.dll", SetLastError = false, CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
        public static extern void PostQuitMessage([Optional] int nExitCode);


        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);



        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern ushort RegisterClassEx(in WNDCLASSEX Arg1);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx([In] WindowStylesEx dwExStyle, string lpClassName,
          string lpWindowName, [In] WindowStyles dwStyle, int x, int y, int nWidth, int nHeight,
          IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static IntPtr CreateWindow(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        public delegate IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public WindowClassStyles style;
            public WindowProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }
        [Flags]
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x0,
            WS_SYSMENU = 0x00080000,
            WS_BORDER = 0x00800000,
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000,
            WS_POPUP = 0x80000000,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        }

        [Flags]
        public enum WindowStylesEx : uint
        {
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_NOACTIVATE = 0x08000000,
        }
        [Flags]
        public enum WindowClassStyles : uint
        {
            CS_DBLCLKS = 0x8u,
        }

        [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);



        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetSystemMetrics(int nIndex);
    }
}
