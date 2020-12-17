using Caliburn.Micro;
using ErogeHelper.Model;
using ErogeHelper.ViewModels;
using ErogeHelper.Views;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ErogeHelper.Common.Helper
{
    class GameHooker
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GameHooker));

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
            gameProc.Exited += new EventHandler(ApplicationExit); // is there has 

            gameHWnd = gameProc.MainWindowHandle;

            log.Info($"Set handle to 0x{Convert.ToString(gameHWnd.ToInt64(), 16).ToUpper()} Title: {gameProc.MainWindowTitle}");
            uint targetThreadId = NativeMethods.GetWindowThread(gameHWnd);

            if (gameHWnd != IntPtr.Zero)
            {
                // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数
                hWinEventHook = NativeMethods.WinEventHookOne(NativeMethods.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE,
                                                     WinEventDelegate,
                                                     (uint)gameProc.Id,
                                                     targetThreadId);
                // Send game position first time
                UpdateLocation();
                return;
            }

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

        private static void UpdateLocation()
        {
            var rect = NativeMethods.GetWindowRect(gameHWnd, DataRepository.dpi);
            var rectClient = NativeMethods.GetClientRect(gameHWnd, DataRepository.dpi);

            var width = rect.Right - rect.Left;  // rectClient.Right + shadow*2
            var height = rect.Bottom - rect.Top; // rectClient.Bottom + shadow + title

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
        }

        private static void ApplicationExit(object? sender, EventArgs e)
        {
            log.Info("Detected game quit event");
            GCSafetyHandle.Free();
            NativeMethods.WinEventUnhook(hWinEventHook);

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => Application.Current.Shutdown()));
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
