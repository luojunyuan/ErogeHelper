//using ErogeHelper.Common.Contract;
//using ErogeHelper.Common.Entity;
//using ErogeHelper.Model.Service.Interface;
//using Splat;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Windows;
//using System.Windows.Threading;
//using Vanara.PInvoke;

//namespace ErogeHelper.Model.Service
//{
//    class GameWindowHooker : IGameWindowHooker, IEnableLogger
//    {
//        public event EventHandler<GameWindowPositionEventArgs>? GamePosChanged;

//        public IntPtr GameRealHwnd => _realWindowHandle;

//        public void SetGameWindowHook(Process process)
//        {
//            _gameProc = process;

//            _winEventDelegate = WinEventCallback;
//            _gcSafetyHandle = GCHandle.Alloc(_winEventDelegate);

//            _gameProc.EnableRaisingEvents = true;
//            _gameProc.Exited += ApplicationExit;

//            _realWindowHandle = FindRealHandle();
//            SetWindowHandler();
//        }

//        public void ResetWindowHandler()
//        {
//            //if (_realWindowHandle != IntPtr.Zero)
//            //{
//            //    User32.GetWindowRect(_realWindowHandle, out var windowRect);
//            //    if (windowRect.Equals(MinimizedPosition))
//            //    {
//            //        this.Log().Debug("window minimized");
//            //        return;
//            //    }
//            //}

//            //HWND handle = FindRealHandle();

//            //if (handle.Equals(_realWindowHandle))
//            //{
//            //    UpdateLocation();
//            //}
//            //else if (handle != IntPtr.Zero)
//            //{
//            //    _realWindowHandle = handle;
//            //    SetWindowHandler();
//            //    UpdateLocation();
//            //}
//            //else
//            //{
//            //    throw new InvalidOperationException("Not found game window");
//            //}
//        }

//        public void InvokeUpdatePosition() => UpdateLocation();

//        private User32.WinEventProc? _winEventDelegate;
//        private User32.SafeEventHookHandle? _hWinEventHook;
//        private GCHandle _gcSafetyHandle;
//        private Process _gameProc = new();
//        private IntPtr _realWindowHandle;

//        private static RECT MinimizedPosition = new() { left = -32000, top = -32000, right = -31840, bottom = -37972 };
//        private static readonly GameWindowPositionEventArgs HiddenPos = new()
//        {
//            ClientArea = new Thickness(),
//            Left = -32000,
//            Top = -32000,
//            Height = 0,
//            Width = 0,
//        };

//        private async void SetWindowHandler()
//        {
//            this.Log().Debug($"Set handle to 0x{Convert.ToString(_realWindowHandle.ToInt32(), 16).ToUpper()} " +
//                             $"Title: {_gameProc.MainWindowTitle}");
//            var gameUIThreadId = User32.GetWindowThreadProcessId(_realWindowHandle, out _);

//            // Hook must register in message loop thread
//            await Dispatcher.InvokeAsync(() =>
//            {
//                _hWinEventHook = User32.SetWinEventHook(
//                    User32.WindowsEventHookType.EVENT_OBJECT_FOCUS, 
//                    User32.WindowsEventHookType.EVENT_OBJECT_LOCATIONCHANGE, 
//                    IntPtr.Zero, _winEventDelegate, 
//                    _gameProc.Id, gameUIThreadId,
//                    User32.WindowsEventHookFlags.WINEVENT_INCONTEXT |
//                    User32.WindowsEventHookFlags.WINEVENT_SKIPOWNPROCESS);
//                if (!_hWinEventHook.IsInvalid)
//                    throw new InvalidOperationException("Install hook failed");
//            });
//        }
        
//        private const int SWEH_CHILDID_SELF = 0;

//        private void WinEventCallback(IntPtr hWinEventHook,
//                                      User32.WindowsEventHookType @event,
//                                      IntPtr hwnd,
//                                      int idObject,
//                                      int idChild,
//                                      int dwEventThread,
//                                      uint dwmsEventTime)
//        {
//            // When game window get focus
//            if (hwnd.Equals(_realWindowHandle) &&
//                @event == User32.WindowsEventHookType.EVENT_OBJECT_FOCUS &&
//                idObject == SWEH_CHILDID_SELF)
//            {
//                UpdateLocation();
//            }

//            // When game window location changed
//            if (hwnd.Equals(_realWindowHandle) &&
//                @event == User32.WindowsEventHookType.EVENT_OBJECT_FOCUS &&
//                idObject == SWEH_CHILDID_SELF)
//            {
//                UpdateLocation();
//            }
//        }

//        private void UpdateLocation()
//        {
//            if (_realWindowHandle == IntPtr.Zero)
//            {
//                GamePosChanged?.Invoke(this, HiddenPos);
//                return;
//            }
            
//            User32.GetWindowRect(_realWindowHandle, out var rectWindow);
//            User32.GetClientRect(_realWindowHandle, out var rectClient);

//            var width = rectWindow.right - rectWindow.left;  // equal rectClient.Right + shadow*2
//            var height = rectWindow.bottom - rectWindow.top; // equal rectClient.Bottom + shadow + title

//            var winShadow = (width - rectClient.right) / 2;

//            var wholeHeight = rectWindow.bottom - rectWindow.top;
//            var winTitleHeight = wholeHeight - rectClient.bottom - winShadow;

//            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

//            if (rectWindow.Equals(MinimizedPosition))
//            {
//                this.Log().Debug("window minimized");
//                //return;
//            }

//            GamePosChanged?.Invoke(this, new GameWindowPositionEventArgs
//            {
//                Height = height,
//                Width = width,
//                Left = rectWindow.left,
//                Top = rectWindow.top,
//                ClientArea = clientArea
//            });
//        }

//        private IntPtr FindRealHandle()
//        {
//            IntPtr targetHandle = IntPtr.Zero;

//            User32.EnumWindows((handle, _) =>
//            {
//                User32.GetWindowRect(handle, out var windowRect);
//                User32.GetWindowThreadProcessId(handle, out var processId);
//                if (_gameProc.Id == processId &&
//                    IsGoodWindow(handle))
//                {
//                    targetHandle = handle;
//                    return false;
//                }
//                return true;
//            }, IntPtr.Zero);

//            return targetHandle;
//        }

//        private static bool IsGoodWindow(IntPtr handle)
//        {
//            if (User32.IsIconic(handle))
//            { 
//                return true;
//            }
//            if (!User32.IsWindowVisible(handle))
//            { 
//                return false;
//            }
//            User32.GetWindowRect(handle, out var windowRect);
//            var (width, height) = (windowRect.right - windowRect.left, windowRect.bottom - windowRect.top);
//            return width >= ConstantValues.GoodWindowWidth && height >= ConstantValues.GoodWindowHeight;
//        }

//        private void ApplicationExit(object? sender, EventArgs e)
//        {
//            this.Log().Debug("Detected game quit event");
//            GamePosChanged?.Invoke(this, HiddenPos);
//            _gcSafetyHandle.Free();
//            _hWinEventHook?.Dispose();

//            Dispatcher.InvokeShutdown();
//        }

//        // For running in the unit test
//        private static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
//    }
//}
