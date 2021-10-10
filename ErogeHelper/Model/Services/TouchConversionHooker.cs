using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services
{
    public class TouchConversionHooker : ITouchConversionHooker
    {
        private User32.SafeHHOOK? _hookId;
        private readonly User32.HookProc _hookCallback;

        // User32.MOUSEEVENTF.MOUSEEVENTF_FROMTOUCH
        private const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        public TouchConversionHooker(IGameInfoRepository? ehDbRepository = null)
        {
            _hookCallback = HookCallback;
            Enable = (ehDbRepository ?? DependencyResolver.GetService<IGameInfoRepository>()).GameInfo!.IsEnableTouchToMouse;
        }

        public bool Enable { get; set; }

        public void Init()
        {
            var moduleHandle = Kernel32.GetModuleHandle();

            _hookId = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, _hookCallback, moduleHandle, 0);
            if (_hookId == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var obj = Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
                if (obj is null) return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

                var info = (User32.MSLLHOOKSTRUCT)obj;

                var extraInfo = (uint)info.dwExtraInfo;
                if (Enable &&
                    (extraInfo & MOUSEEVENTF_FROMTOUCH) == MOUSEEVENTF_FROMTOUCH)
                {
                    switch ((int)wParam)
                    {
                        case 0x202:
                            Task.Run(async () =>
                            {
                                var (X, Y) = GetCursorPosition();
                                User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN, X, Y, 0, IntPtr.Zero);
                                await Task.Delay(50);
                                User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP, X, Y, 0, IntPtr.Zero);
                            });
                            break;
                        case 0x205:
                            Task.Run(async () =>
                            {
                                var (X, Y) = GetCursorPosition();
                                User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN, X, Y, 0, IntPtr.Zero);
                                await Task.Delay(50);
                                User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTUP, X, Y, 0, IntPtr.Zero);
                            });
                            break;
                        default:
                            break;
                    }

                    return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
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
    }
}
