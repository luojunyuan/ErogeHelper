using System;
using System.ComponentModel;
using System.Drawing;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services
{
    public class TouchConversionHooker : ITouchConversionHooker, IDisposable
    {
        private User32.SafeHHOOK? _hookId;
        private readonly User32.HookProc _hookCallback;

        // User32.MOUSEEVENTF.MOUSEEVENTF_FROMTOUCH
        private const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        public TouchConversionHooker(IGameInfoRepository? ehDbRepository = null)
        {
            _hookCallback = HookCallback;
            Enable = (ehDbRepository ?? DependencyResolver.GetService<IGameInfoRepository>()).GameInfo.IsEnableTouchToMouse;
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
            if (!Enable || nCode < 0)
            {
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
            
            var obj = Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
            if (obj is null) return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

            var info = (User32.MSLLHOOKSTRUCT)obj;

            var extraInfo = (uint)info.dwExtraInfo;
            if ((extraInfo & MOUSEEVENTF_FROMTOUCH) == MOUSEEVENTF_FROMTOUCH)
            {
                switch ((int)wParam)
                {
                    case 0x202:
                        Observable.Start(() =>
                        {
                            var (x, y) = GetCursorPosition();
                            User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN, x, y, 0, IntPtr.Zero);
                            Thread.Sleep(ConstantValue.MouseDownUpIntervalTime);
                            User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP, x, y, 0, IntPtr.Zero);
                        });
                        break;
                    case 0x205:
                        Observable.Start(() =>
                        {
                            var (x, y) = GetCursorPosition();
                            User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN, x, y, 0, IntPtr.Zero);
                            Thread.Sleep(ConstantValue.MouseDownUpIntervalTime);
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
}
