using AutoHotkey.Interop;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ErogeHelper.Model.Service
{
    public class TouchConversionHooker : ITouchConversionHooker, IDisposable
    {
        public TouchConversionHooker()
        {
            _hookCallback = HookCallback;
        }

        private readonly NativeMethods.LowLevelMouseProc _hookCallback;

        public void Init()
        {
            var moduleHandle = NativeMethods.GetModuleHandle();

            _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _hookCallback, moduleHandle, 0);
            if (_hookId == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private IntPtr _hookId = IntPtr.Zero;
        private readonly AutoHotkeyEngine _ahk = AutoHotkeyEngine.Instance;

        public bool Enable { get; set; }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var obj = Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHook));
                if (obj is not null)
                {
                    var info = (NativeMethods.MSLLHook)obj;

                    var extraInfo = (uint)info.DwExtraInfo.ToInt64();
                    if ((extraInfo & NativeMethods.MOUSEEVENTF_FROMTOUCH) == NativeMethods.MOUSEEVENTF_FROMTOUCH
                        && Enable)
                    {
                        TouchEventFilter(new TouchInfo { Point = info.Point, Token = (int)wParam });
                        if (_press)
                        {
                            NativeMethods.MoveCursorToPoint(_pos4.Point.X, _pos4.Point.Y);
                            _ahk.ExecRaw("MouseClick, left");
                        }

                        if (_longPress)
                        {
                            NativeMethods.MoveCursorToPoint(_pos4.Point.X, _pos4.Point.Y);
                            _ahk.ExecRaw("MouseClick, right");
                        }
                        return new IntPtr(1);
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool _press;
        private bool _longPress;

        private TouchInfo _pos1;
        private TouchInfo _pos2;
        private TouchInfo _pos3;
        private TouchInfo _pos4;

        private void TouchEventFilter(TouchInfo token)
        {
            _pos4 = _pos3;
            _pos3 = _pos2;
            _pos2 = _pos1;
            _pos1 = token;

            switch (_pos4.Token)
            {
                case 0x200 when _pos3.Token == 0x200 && _pos2.Token == 0x201 && _pos1.Token == 0x202:
                case 0x200 when _pos3.Token == 0x200 && _pos2.Token == 0x200 && _pos1.Token == 0x202:
                    _press = true;
                    break;
                case 0x200 when _pos3.Token == 0x200 && _pos2.Token == 0x204 && _pos1.Token == 0x205:
                    _longPress = true;
                    break;
                default:
                    _press = false;
                    _longPress = false;
                    break;
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            NativeMethods.UnhookWindowsHookEx(_hookId);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~TouchConversionHooker()
        {
            Dispose();
        }
    }
}