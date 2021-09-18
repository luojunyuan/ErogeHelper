using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services
{
    public class GameWindowHooker : IGameWindowHooker, IEnableLogger
    {
        private HWND _gameHwnd;
        private Process _gameProc = new();
        private static readonly GameWindowPositionPacket HiddenPos = new(0, 0, -32000, -32000, new Thickness());
        private static readonly RECT MinimizedPosition = new(-32000, -32000, -31840, -31972);
        private static readonly RECT MinimizedPosition4K = new(-64000, -64000, -63680, -63944);

        private User32.HWINEVENTHOOK _windowsEventHook = IntPtr.Zero;
        private const uint EventObjectDestroy = 0x8001;
        private const uint EventObjectShow = 0x8002;
        private const uint EventObjectHide = 0x8003;
        private const uint EventObjectReorder = 0x8004;
        private const uint EventObjectFocus = 0x8005;
        private const uint EventObjectLocationChange = 0x800B;
        private GCHandle _gcSafetyHandle;
        private User32.WinEventProc? _winEventDelegate;
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook
        private static readonly User32.WINEVENT WinEventHookInternalFlags =
                                User32.WINEVENT.WINEVENT_INCONTEXT |
                                User32.WINEVENT.WINEVENT_SKIPOWNPROCESS;
        private const long SWEH_CHILDID_SELF = 0;

        private readonly ReplaySubject<GameWindowPositionPacket> _gamePositionSubject = new(1);
        public IObservable<GameWindowPositionPacket> GamePosUpdated => _gamePositionSubject.AsObservable();

        private IGameDataService? _gameDataService;

        public void SetGameWindowHook(Process process)
        {
            _gameProc = process;

            _gameProc.EnableRaisingEvents = true;
            _gameDataService = DependencyInject.GetService<IGameDataService>(); 
            _gameDataService.MainWindowHandle = _gameHwnd = CurrentWindowHandle(_gameProc);

            var targetThreadId = User32.GetWindowThreadProcessId(_gameHwnd, out var processId);

            _winEventDelegate = WinEventCallback;
            _gcSafetyHandle = GCHandle.Alloc(_winEventDelegate);

            _windowsEventHook = User32.SetWinEventHook(
                EventObjectDestroy, EventObjectLocationChange,
                IntPtr.Zero, _winEventDelegate, processId, targetThreadId,
                WinEventHookInternalFlags);

            _gameProc.Events().Exited
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.Log().Debug("Detected game quit event");
                    _gamePositionSubject.OnNext(HiddenPos);
                    _gamePositionSubject.OnCompleted();
                    _gcSafetyHandle.Free();
                    User32.UnhookWinEvent(_windowsEventHook);

                    Application.Current.Windows
                        .Cast<Window>().ToList()
                        .ForEach(w => w.Close());

                    //var gameInfo = DependencyInject.GetService<IEhDbRepository>().GameInfo;
                    //if (gameInfo is not null && gameInfo.UseCloudSave)
                    //{
                    //    DependencyInject.GetService<ISavedataSyncService>().UpdateSync();
                    //}

                    App.Terminate();
                });
        }

        public void InvokeUpdatePosition() => UpdateLocation();

        private static HWND CurrentWindowHandle(Process proc)
        {
            proc.WaitForInputIdle(ConstantValues.WaitGameStartTimeout);
            proc.Refresh();
            var gameHwnd = proc.MainWindowHandle;

            User32.GetClientRect(gameHwnd, out var clientRect);

            if (clientRect.bottom > ConstantValues.GoodWindowHeight &&
                clientRect.right > ConstantValues.GoodWindowWidth)
            {
                return gameHwnd;
            }
            else
            {
                // TODO: Improve finding handle when game minimized 
                if (User32.IsIconic(proc.MainWindowHandle))
                {
                    throw new InvalidOperationException("make sure game is in front");
                }

                var spendTime = new Stopwatch();
                spendTime.Start();
                while (spendTime.Elapsed.TotalMilliseconds < ConstantValues.WaitGameStartTimeout)
                {
                    if (proc.HasExited)
                        return IntPtr.Zero;

                    var handles = GetRootWindowsOfProcess(proc.Id);
                    foreach (var handle in handles)
                    {
                        User32.GetClientRect(handle, out clientRect);
                        if (clientRect.bottom > ConstantValues.GoodWindowHeight &&
                            clientRect.right > ConstantValues.GoodWindowWidth)
                        {
                            LogHost.Default.Debug($"Set new handle 0x{handle.DangerousGetHandle():X8}");
                            return handle;
                        }
                    }
                    Thread.Sleep(ConstantValues.MinimumLagTime);
                }
                throw new ArgumentException("Find window handle failed");
            }
        }

        private void WinEventCallback(
            User32.HWINEVENTHOOK hWinEventHook,
            uint eventType,
            HWND hWnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            switch (eventType)
            {
                // TODO: Show and Hide MainGameWindow with these three events
                case EventObjectDestroy:
                    {
                        if (hWnd == _gameHwnd)
                        {
                            try
                            {
                                _gameDataService!.MainWindowHandle = _gameHwnd = CurrentWindowHandle(_gameProc);
                                this.Log().Debug("Game window show - recreate");
                                UpdateLocation();
                            }
                            catch (InvalidOperationException ex)
                            {
                                // game process may get already exit
                                this.Log().Debug(ex.Message);
                            }
                        }
                    }
                    break;
                case EventObjectShow:
                    {
                        if (_gameProc.MainWindowHandle != hWnd && hWnd == _gameHwnd)
                        {
                            this.Log().Debug("Game window show");
                            UpdateLocation();
                        }
                    }
                    break;
                case EventObjectHide:
                    {
                        if (_gameProc.MainWindowHandle != hWnd && hWnd == _gameHwnd)
                        {
                            this.Log().Debug("Game window hide");
                        }
                    }
                    break;
                case EventObjectLocationChange:
                    {
                        // Update game's position information
                        if (hWnd == _gameHwnd && idObject == SWEH_CHILDID_SELF)
                        {
                            UpdateLocation();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateLocation()
        {
            User32.GetWindowRect(_gameHwnd, out var rect);
            User32.GetClientRect(_gameHwnd, out var rectClient);

            var width = rect.right - rect.left;  // equal rectClient.Right + shadow*2
            var height = rect.bottom - rect.top; // equal rectClient.Bottom + shadow + title

            var winShadow = (width - rectClient.right) / 2;

            var wholeHeight = rect.bottom - rect.top;
            var winTitleHeight = wholeHeight - rectClient.bottom - winShadow;

            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            _gamePositionSubject.OnNext(new GameWindowPositionPacket(height, width, rect.left, rect.top, clientArea));
        }

        private static IEnumerable<HWND> GetRootWindowsOfProcess(int pid)
        {
            IEnumerable<HWND> rootWindows = GetChildWindows(IntPtr.Zero);
            List<HWND> dsProcRootWindows = new();
            foreach (var hWnd in rootWindows)
            {
                _ = User32.GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        private static IEnumerable<HWND> GetChildWindows(HWND parent)
        {
            List<HWND> result = new();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                static bool childProc(HWND handle, IntPtr pointer)
                {
                    var gch = GCHandle.FromIntPtr(pointer);
                    if (gch.Target is not List<HWND> list)
                    {
                        throw new InvalidCastException("GCHandle Target could not be cast as List<HWND>");
                    }
                    list.Add(handle);
                    return true;
                }
                User32.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }
    }
}
