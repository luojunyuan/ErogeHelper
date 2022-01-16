using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Structs;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services;

public class GameWindowHooker : IGameWindowHooker, IEnableLogger
{
    private HWND _gameHwnd;
    private Process _gameProc = null!;
    private static readonly WindowPosition HiddenPos = new(0, 0, -32000, -32000);

    private User32.HWINEVENTHOOK _windowsEventHook = IntPtr.Zero;
    private const uint EventObjectDestroy = 0x8001;
    private const uint EventObjectShow = 0x8002;
    private const uint EventObjectHide = 0x8003;
    private const uint EventObjectLocationChange = 0x800B;
    private GCHandle _gcSafetyHandle;
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook
    private const User32.WINEVENT WinEventHookInternalFlags = User32.WINEVENT.WINEVENT_INCONTEXT |
                                                              User32.WINEVENT.WINEVENT_SKIPOWNPROCESS;
    private const long SWEH_CHILDID_SELF = 0;

    private readonly ReplaySubject<WindowPosition> _gamePositionSubj = new(1);
    public IObservable<WindowPosition> GamePosUpdated => _gamePositionSubj;

    private readonly ReplaySubject<WindowPositionDelta> _gamePosChangedSubj = new(1);
    public IObservable<WindowPositionDelta> GamePosChanged => _gamePosChangedSubj;

    private readonly Subject<ViewOperation> _ViewOperationSubj = new();
    public IObservable<ViewOperation> WhenViewOperated => _ViewOperationSubj;

    private IGameDataService? _gameDataService;
    public void SetupGameWindowHook(Process process, IGameDataService? gameDataService, IScheduler mainScheduler)
    {
        _gameProc = process;

        _gameProc.EnableRaisingEvents = true;
        _gameDataService = gameDataService ?? DependencyResolver.GetService<IGameDataService>();
        _gameHwnd = CurrentWindowHandle(_gameProc);
        _gameDataService.SetGameRealWindowHandle(_gameHwnd);

        var targetThreadId = User32.GetWindowThreadProcessId(_gameHwnd, out var processId);

        User32.WinEventProc winEventDelegate = WinEventCallback;
        _gcSafetyHandle = GCHandle.Alloc(winEventDelegate);

        mainScheduler.Schedule(() =>
            _windowsEventHook = User32.SetWinEventHook(
                 EventObjectDestroy, EventObjectLocationChange,
                 IntPtr.Zero, winEventDelegate, processId, targetThreadId,
                 WinEventHookInternalFlags));

        _gameProc.Exited += (_, _) =>
        {
            // Do cloud save check

            mainScheduler.Schedule(() => _gamePositionSubj.OnNext(HiddenPos));
            this.Log().Debug("Detected game quit event");
            _gcSafetyHandle.Free();
            // Produce EventObjectDestroy
            User32.UnhookWinEvent(_windowsEventHook);

            _ViewOperationSubj.OnNext(ViewOperation.TerminateApp);
        };
    }

    public WindowPosition InvokeUpdatePosition() => UpdateLocation();

    private static HWND CurrentWindowHandle(Process proc, bool activeGame = true)
    {
        proc.WaitForInputIdle(ConstantValue.WaitGameStartTimeout);
        proc.Refresh();
        var gameHwnd = proc.MainWindowHandle;

        if (activeGame && User32.IsIconic(proc.MainWindowHandle))
        {
            User32.ShowWindow(proc.MainWindowHandle, ShowWindowCommand.SW_RESTORE);
        }

        User32.GetClientRect(gameHwnd, out var clientRect);

        if (clientRect.bottom > ConstantValue.GoodWindowHeight &&
            clientRect.right > ConstantValue.GoodWindowWidth)
        {
            return gameHwnd;
        }
        else
        {
            var spendTime = new Stopwatch();
            spendTime.Start();
            while (spendTime.Elapsed.TotalMilliseconds < ConstantValue.WaitGameStartTimeout)
            {
                if (proc.HasExited)
                    return IntPtr.Zero;

                var handles = GetRootWindowsOfProcess(proc.Id);
                foreach (var handle in handles)
                {
                    User32.GetClientRect(handle, out clientRect);
                    if (clientRect.bottom > ConstantValue.GoodWindowHeight &&
                        clientRect.right > ConstantValue.GoodWindowWidth)
                    {
                        LogHost.Default.Debug($"Set new handle 0x{handle.DangerousGetHandle():X8}");
                        return handle;
                    }
                }
                Thread.Sleep(ConstantValue.UIMinimumResponseTime);
            }
            throw new ArgumentException("Find window handle failed");
        }
    }

    /// <summary>
    /// Running in UI thread
    /// </summary>
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
            case EventObjectDestroy:
                {
                    // even if game lost focus would get EventObjectDestroy object (krkr)
                    if (hWnd == _gameHwnd)
                    {
                        try
                        {
                            _gameHwnd = CurrentWindowHandle(_gameProc, false);
                            if (_gameHwnd.IsNull)
                            {
                                _gamePositionSubj.OnNext(HiddenPos);
                                _gamePositionSubj.OnCompleted();
                            }
                            else
                            {
                                _gameDataService!.SetGameRealWindowHandle(_gameHwnd);
                                _ViewOperationSubj.OnNext(ViewOperation.Show);
                                this.Log().Debug("Game window show - recreate");
                                UpdateLocation();
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // game process may get already exit, caused by Process.WaitForInputIdle() with delay
                            this.Log().Debug(ex.Message);
                        }
                    }
                }
                break;
            case EventObjectShow:
                {
                    if (_gameProc.MainWindowHandle != hWnd && hWnd == _gameHwnd)
                    {
                        _ViewOperationSubj.OnNext(ViewOperation.Show);
                        this.Log().Debug("Game window show");
                        UpdateLocation();
                    }
                }
                break;
            case EventObjectHide:
                {
                    if (_gameProc.MainWindowHandle != hWnd && hWnd == _gameHwnd)
                    {
                        _ViewOperationSubj.OnNext(ViewOperation.Hide);
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
        }
    }

    private WindowPosition _lastPos = HiddenPos;

    private WindowPosition UpdateLocation()
    {
        User32.GetWindowRect(_gameHwnd, out var rect);
        User32.GetClientRect(_gameHwnd, out var rectClient);

        var winShadow = (rect.Width - rectClient.right) / 2;
        var left = rect.left + winShadow;

        var wholeHeight = rect.bottom - rect.top;
        var winTitleHeight = wholeHeight - rectClient.bottom - winShadow;
        var top = rect.top + winTitleHeight;

        var windowPosition = new WindowPosition(rectClient.Height, rectClient.Width, left, top);

        if (windowPosition.Left == 0 && windowPosition.Top == 0 &&
            windowPosition.Height == 0 && windowPosition.Width == 0 && !_gameProc.HasExited)
        {
            // SST311212 MessageBox
            _gameHwnd = CurrentWindowHandle(_gameProc, false);
            _gameDataService!.SetGameRealWindowHandle(_gameHwnd);
        }

        _gamePosChangedSubj.OnNext(new WindowPositionDelta(left - _lastPos.Left, top - _lastPos.Top));
        _lastPos = windowPosition;
        _gamePositionSubj.OnNext(windowPosition);
        return windowPosition;
    }

    private static IEnumerable<HWND> GetRootWindowsOfProcess(int pid)
    {
        var rootWindows = GetChildWindows(IntPtr.Zero);
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
            static bool ChildProc(HWND handle, IntPtr pointer)
            {
                var gch = GCHandle.FromIntPtr(pointer);
                if (gch.Target is not List<HWND> list)
                {
                    throw new InvalidCastException("GCHandle Target could not be cast as List<HWND>");
                }
                list.Add(handle);
                return true;
            }
            User32.EnumChildWindows(parent, ChildProc, GCHandle.ToIntPtr(listHandle));
        }
        finally
        {
            if (listHandle.IsAllocated)
                listHandle.Free();
        }
        return result;
    }
}
