using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class GameWindowHooker : IGameWindowHooker
    {
        public event Action<GameWindowPosition>? GamePosArea;

        public GameWindowHooker(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public async Task SetGameWindowHookAsync()
        {
            _gameProc = _ehConfigRepository.MainProcess;
            _gameHWnd = _gameProc.MainWindowHandle;

            await Task.Run(() =>
            {
                _winEventDelegate = WinEventCallback;
                _gcSafetyHandle = GCHandle.Alloc(_winEventDelegate);

                // Register game exit event
                _gameProc.EnableRaisingEvents = true;
                _gameProc.Exited += ApplicationExit;

                ResetWindowHandler();
                // For the first time
                if (_gameHWnd == _gameProc.MainWindowHandle)
                {
                    SetWindowHandler();
                }
            });
        }

        public void ResetWindowHandler()
        {
            var clientRect = NativeMethods.GetClientRect(_gameHWnd);
            var realHandle = IntPtr.Zero;

            if (400 > clientRect.Bottom && 400 > clientRect.Right)
            {
                // Tip: This would active even when game window get minimize
                var windowRect = NativeMethods.GetWindowRect(_gameHWnd);
                if ($"{windowRect.Left} {windowRect.Top} {windowRect.Right} {windowRect.Bottom}"
                    .Equals(MinimizedPosition))
                {
                    // Ignore minimize window situation
                    return;
                }

                // Start search handle
                realHandle = FindRealHandle();
            }

            if (realHandle != IntPtr.Zero)
            {
                _gameHWnd = realHandle;
                SetWindowHandler();
            }
            else
            {
                // gameHWnd still be last handle
            }
        }

        private NativeMethods.WinEventDelegate? _winEventDelegate;
        private GCHandle _gcSafetyHandle;
        private IntPtr _hWinEventHook = IntPtr.Zero;
        private Process _gameProc = new ();
        private IntPtr _gameHWnd;
        private const string MinimizedPosition = "-32000 -32000 -31840 -31972";
        public static readonly GameWindowPosition HiddenPos = new()
        {
            ClientArea = new Thickness(),
            Left = -32000,
            Top = -32000,
            Height = 0,
            Width = 0,
        };

        private void SetWindowHandler()
        {
            Log.Info($"Set handle to 0x{Convert.ToString(_gameHWnd.ToInt64(), 16).ToUpper()} " +
                     $"Title: {_gameProc.MainWindowTitle}");
            var targetThreadId = NativeMethods.GetWindowThread(_gameHWnd);

            // XXX: Application.Current made tightly coupled to wpf System.Windows.Application and hard to test
            // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数，必须在UI线程上执行启用
            Dispatcher.InvokeAsync(
                () => _hWinEventHook = NativeMethods.WinEventHookOne(
                    NativeMethods.SWEHEvents.EventObjectLocationChange,
                    _winEventDelegate!,
                    (uint)_gameProc.Id,
                    targetThreadId)
            );

            // Send game position first time
            UpdateLocation();
        }

        private void WinEventCallback(IntPtr hWinEventHook,
            NativeMethods.SWEHEvents eventType,
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
            //    log.Info("Game window get focus");
            //}

            // Update game's position information
            if (hWnd == _gameHWnd &&
                eventType == NativeMethods.SWEHEvents.EventObjectLocationChange &&
                idObject == NativeMethods.SWEH_CHILDID_SELF)
            {
                UpdateLocation();
            }
        }

        private void UpdateLocation()
        {
            var rect = NativeMethods.GetWindowRect(_gameHWnd);
            var rectClient = NativeMethods.GetClientRect(_gameHWnd);

            var width = rect.Right - rect.Left;  // equal rectClient.Right + shadow*2
            var height = rect.Bottom - rect.Top; // equal rectClient.Bottom + shadow + title

            var winShadow = (width - rectClient.Right) / 2;

            var wholeHeight = rect.Bottom - rect.Top;
            var winTitleHeight = wholeHeight - rectClient.Bottom - winShadow;

            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            GamePosArea?.Invoke(new GameWindowPosition
            {
                Height = height,
                Width = width,
                Left = rect.Left,
                Top = rect.Top,
                ClientArea = clientArea
            });

            #region Change FloatButton Position
            //if (oldWidth == -1 && oldHeight == -1)
            //{
            //    oldWidth = width;
            //    oldHeight = height;
            //}
            //else if (oldHeight != height || oldWidth != width)
            //{
            //    UpdateButtonPosEvent?.Invoke(typeof(GameHooker), rectClient.Bottom, rectClient.Right);
            //    oldHeight = height;
            //    oldWidth = width;
            //}
            #endregion
        }

        private IntPtr FindRealHandle()
        {
            var textLength = _gameProc.MainWindowTitle.Length;
            var title = new StringBuilder(textLength + 1);
            NativeMethods.GetWindowText(_gameProc.MainWindowHandle, title, title.Capacity);

            Log.Info($"Can't find standard window in MainWindowHandle! Start search title 「{title}」");

            // Must use original gameProc.MainWindowHandle
            var first = NativeMethods.GetWindow(_gameProc.MainWindowHandle, NativeMethods.GW.HWNDFIRST);
            var last = NativeMethods.GetWindow(_gameProc.MainWindowHandle, NativeMethods.GW.HWNDLAST);

            var cur = first;
            // 遍历所有窗口标题(TODO: limit in gameProc only)
            while (cur != last)
            {
                var outText = new StringBuilder(textLength + 1);
                NativeMethods.GetWindowText(cur, outText, title.Capacity);
                if (outText.Equals(title))
                {
                    var rectClient = NativeMethods.GetClientRect(cur);
                    if (rectClient.Right != 0 && rectClient.Bottom != 0)
                    {
                        // check pid
                        _ = NativeMethods.GetWindowThread(cur, out uint pid);
                        if (_ehConfigRepository.GameProcesses.Any(proc => proc.Id == pid))
                        {
                            Log.Info($"Find handle at 0x{Convert.ToString(cur.ToInt64(), 16).ToUpper()}");
                            // Search over, believe handle is found
                            return cur;
                        }
                    }
                }

                cur = NativeMethods.GetWindow(cur, NativeMethods.GW.HWNDNEXT);
            }

            Log.Info("Find failed, use last handle");
            return IntPtr.Zero;
        }

        private void ApplicationExit(object? sender, EventArgs e)
        {
            Log.Info("Detected game quit event");
            GamePosArea?.Invoke(HiddenPos);
            _gcSafetyHandle.Free();
            NativeMethods.WinEventUnhook(_hWinEventHook);

            Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
        }

        // For running in the unit test
        private static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}