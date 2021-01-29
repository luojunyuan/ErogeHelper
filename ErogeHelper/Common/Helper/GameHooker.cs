using Caliburn.Micro;
using ErogeHelper.Model;
using Serilog;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace ErogeHelper.Common.Helper
{
    class GameHooker
    {
        private static readonly Process gameProc = DataRepository.MainProcess!;

        public static void Init()
        {
            SetGameWindowHook();
        }

        private static IntPtr gameHWnd;

        protected static NativeMethods.WinEventDelegate? WinEventDelegate;
        private static GCHandle GCSafetyHandle;
        private static IntPtr hWinEventHook;
        private static void SetGameWindowHook()
        {
            WinEventDelegate = new NativeMethods.WinEventDelegate(WinEventCallback);
            GCSafetyHandle = GCHandle.Alloc(WinEventDelegate);

            // Register game exit event
            gameProc.EnableRaisingEvents = true;
            gameProc.Exited += new EventHandler(ApplicationExit);

            gameHWnd = gameProc.MainWindowHandle;

            CheckWindowHandler();
            SetWindowHandler();
        }

        public static void CheckWindowHandler()
        {
            var defaultRect = NativeMethods.GetClientRect(gameHWnd);
            if (400 > defaultRect.Bottom && 400 > defaultRect.Right)
            {
                // Tip: 如果MainWindowHandle所在窗口最小化了也会进入这里
                IntPtr realHandle = IntPtr.Zero;

                int textLength = gameProc.MainWindowTitle.Length;
                StringBuilder title = new StringBuilder(textLength + 1);
                NativeMethods.GetWindowText(gameProc.MainWindowHandle, title, title.Capacity);

                Log.Info($"Can't find standard window in MainWindowHandle! Start search title 「{title}」");

                // Must use original gameProc.MainWindowHandle
                IntPtr first = NativeMethods.GetWindow(gameProc.MainWindowHandle, NativeMethods.GW.HWNDFIRST);
                IntPtr last = NativeMethods.GetWindow(gameProc.MainWindowHandle, NativeMethods.GW.HWNDLAST);

                IntPtr cur = first;
                while (cur != last)
                {
                    StringBuilder outText = new StringBuilder(textLength + 1);
                    NativeMethods.GetWindowText(cur, outText, title.Capacity);
                    if (outText.Equals(title))
                    {
                        var rectClient = NativeMethods.GetClientRect(cur);
                        if (rectClient.Right != 0 && rectClient.Bottom != 0)
                        {
                            Log.Info($"Find handle at 0x{Convert.ToString(cur.ToInt64(), 16).ToUpper()}");
                            realHandle = cur;
                            // Search over, believe handle is found
                            break;
                        }
                    }

                    cur = NativeMethods.GetWindow(cur, NativeMethods.GW.HWNDNEXT);
                }

                if (realHandle != IntPtr.Zero)
                {
                    gameHWnd = realHandle;
                    SetWindowHandler();
                }
                else
                {
                    // gameHwnd still be gameProc.MainWindowHandle at first time
                    Log.Info("No realHandle found. Still use last handle");
                }
            }
            
        }

        private static void SetWindowHandler()
        {
            Log.Info($"Set handle to 0x{Convert.ToString(gameHWnd.ToInt64(), 16).ToUpper()} " +
                            $"Title: {gameProc.MainWindowTitle}");
            uint targetThreadId = NativeMethods.GetWindowThread(gameHWnd);

            // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数，必须在UI线程上执行启用
            Application.Current.Dispatcher.InvokeAsync(
                () => hWinEventHook = NativeMethods.WinEventHookOne(
                    NativeMethods.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE,
                    WinEventDelegate!,
                    (uint)gameProc.Id,
                    targetThreadId)
            );
            // Send game position first time
            UpdateLocation();
        }

        protected static void WinEventCallback(IntPtr hWinEventHook,
                                    NativeMethods.SWEH_Events eventType,
                                    IntPtr hWnd,
                                    NativeMethods.SWEH_ObjectId idObject,
                                    long idChild,
                                    uint dwEventThread,
                                    uint dwmsEventTime)
        {
            // 游戏窗口获取焦点时会调用
            //if (hWnd == GameInfo.Instance.hWnd &&
            //    eventType == Hook.SWEH_Events.EVENT_OBJECT_FOCUS)
            //{
            //    log.Info("Game window get foucus");
            //}

            // Update game's position infomation
            if (hWnd == gameHWnd &&
                eventType == NativeMethods.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE &&
                idObject == (NativeMethods.SWEH_ObjectId)NativeMethods.SWEH_CHILDID_SELF)
            {
                UpdateLocation();
            }
        }

        public delegate void GameViewPosEventHandler(object sender, GameViewPlacement e);
        public static event GameViewPosEventHandler? GameViewPosChangedEvent;

        public delegate void UpdateButtonPosEventHandler(object sender, int height, int width);
        public static event UpdateButtonPosEventHandler? UpdateButtonPosEvent;
        static int oldWidth = -1;
        static int oldHeight = -1;

        private static void UpdateLocation()
        {
            var rect = NativeMethods.GetWindowRect(gameHWnd);
            var rectClient = NativeMethods.GetClientRect(gameHWnd);

            var width = rect.Right - rect.Left;  // equal rectClient.Right + shadow*2
            var height = rect.Bottom - rect.Top; // equal rectClient.Bottom + shadow + title

            var winShadow = (width - rectClient.Right) / 2;

            var wholeHeight = rect.Bottom - rect.Top;
            var winTitleHeight = wholeHeight - rectClient.Bottom - winShadow;

            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            GameViewPosChangedEvent?.Invoke(typeof(GameHooker), new GameViewPlacement
            {
                Height = height,
                Width = width,
                Left = rect.Left,
                Top = rect.Top,
                ClientArea = clientArea
            });

            #region Change FloatButton Position
            if (oldWidth == -1 && oldHeight == -1)
            {
                oldWidth = width;
                oldHeight = height;
            }
            else if (oldHeight != height || oldWidth != width)
            {
                UpdateButtonPosEvent?.Invoke(typeof(GameHooker), rectClient.Bottom, rectClient.Right);
                oldHeight = height;
                oldWidth = width;
            }
            #endregion
        }

        private static void ApplicationExit(object? sender, EventArgs e)
        {
            Log.Info("Detected game quit event");
            GCSafetyHandle.Free();
            NativeMethods.WinEventUnhook(hWinEventHook);

            Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
        }
    }

    class GameViewPlacement
    {
        public double Height;
        public double Width;
        public double Left;
        public double Top;
        public Thickness ClientArea;
    }
}
