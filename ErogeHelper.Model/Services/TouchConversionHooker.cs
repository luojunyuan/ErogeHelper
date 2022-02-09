using System.ComponentModel;
using System.Drawing;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services;

public class TouchConversionHooker : ITouchConversionHooker
{
    private readonly User32.SafeHHOOK? _hookId;

    // User32.MOUSEEVENTF.MOUSEEVENTF_FROMTOUCH
    private const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

    private readonly IGameDataService _gameDataService;
    private HWND GameWindowHandle => _gameDataService.GameRealWindowHandle;

    public TouchConversionHooker(
        IGameInfoRepository? gameInfoRepository = null,
        IGameDataService? gameDataService = null)
    {
        _gameDataService = gameDataService ?? DependencyResolver.GetService<IGameDataService>();
        Enable = (gameInfoRepository ?? DependencyResolver.GetService<IGameInfoRepository>()).TryGetGameInfo()?.
            IsEnableTouchToMouse ?? false;

        var moduleHandle = Kernel32.GetModuleHandle();

        _hookId = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, Hook, moduleHandle, 0);
        if (_hookId == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public bool Enable { private get; set; }

    private IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (!Enable || nCode < 0)
        {
            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var obj = Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
        if (obj is not User32.MSLLHOOKSTRUCT info)
        {
            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var extraInfo = (uint)info.dwExtraInfo;
        if ((extraInfo & MOUSEEVENTF_FROMTOUCH) == MOUSEEVENTF_FROMTOUCH)
        {
            var isGameWindow = User32.GetForegroundWindow() == GameWindowHandle;
            if (!isGameWindow)
            {
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            switch ((int)wParam)
            {
                case 0x202:
                    Observable.Start(() =>
                    {
                        var (x, y) = GetCursorPosition();
                        User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN, x, y, 0, IntPtr.Zero);
                        Thread.Sleep(ConstantValue.UserTimerMinimum);
                        User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP, x, y, 0, IntPtr.Zero);
                    });
                    break;
                case 0x205:
                    Observable.Start(() =>
                    {
                        var (x, y) = GetCursorPosition();
                        User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN, x, y, 0, IntPtr.Zero);
                        Thread.Sleep(ConstantValue.UserTimerMinimum);
                        User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTUP, x, y, 0, IntPtr.Zero);
                    });
                    break;
            }
        }

        return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static (int X, int Y) GetCursorPosition()
    {
        var gotPoint = User32.GetCursorPos(out var currentMousePoint);
        if (!gotPoint)
        {
            currentMousePoint = new Point(0, 0);
        }
        return (currentMousePoint.X, currentMousePoint.Y);
    }

    public void Dispose() => User32.UnhookWindowsHookEx(_hookId);
}

/// <summary>
/// For debug proposal
/// </summary>
public class TouchConversionHookerFake : ITouchConversionHooker
{
    public bool Enable { get; set; }

    public void Dispose() { }
}
