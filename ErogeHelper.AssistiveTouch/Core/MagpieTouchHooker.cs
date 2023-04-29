using ErogeHelper.AssistiveTouch.NativeMethods;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace ErogeHelper.AssistiveTouch.Core;

internal class MagpieTouchHooker : IDisposable
{
    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    private readonly User32.HWINEVENTHOOK _windowsEventHook;

    private readonly GCHandle _gcSafetyHandle;

    public MagpieTouchHooker()
    {
        User32.WinEventProc winEventDelegate = WinEventCallback;
        _gcSafetyHandle = GCHandle.Alloc(winEventDelegate);

        _windowsEventHook = User32.SetWinEventHook(
             EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
             IntPtr.Zero, winEventDelegate, 0, 0,
             User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
    }

    private void WinEventCallback(
        User32.HWINEVENTHOOK hWinEventHook,
        uint eventType,
        IntPtr hWnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime)
    {
        if (eventType == EVENT_SYSTEM_FOREGROUND)
        {
            const string HOST_WINDOW_CLASS_NAME = "Window_Magpie_967EB565-6F73-4E94-AE53-00CC42592A22";
            var handle = User32.FindWindow(HOST_WINDOW_CLASS_NAME, null);
            if (IntPtr.Zero == handle)
            {
                Debug.WriteLine("not exist");
            }
            else
            {
                Debug.WriteLine("exist");
            }
        }
    }

    public void Dispose()
    {
        _gcSafetyHandle.Free();
        User32.UnhookWinEvent(_windowsEventHook);
    }

    public static class TouchRepositionHooker
    {
        private const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        private static IntPtr _hookId;

        public static void Install()
        {
            var moduleHandle = Kernel32.GetModuleHandle(); // get current exe instant handle

            _hookId = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, Hook, moduleHandle, 0); // tid 0 set global hook
            if (_hookId == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void UnInstall() => User32.UnhookWindowsHookEx(_hookId);

        private static IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);

            var obj = Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
            if (obj is not User32.MSLLHOOKSTRUCT info)
                return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);

            var extraInfo = (uint)info.dwExtraInfo;
            if ((extraInfo & MOUSEEVENTF_FROMTOUCH) == MOUSEEVENTF_FROMTOUCH)
            {
                // Reposition
                Debug.WriteLine(info.pt);
                switch ((int)wParam)
                {
                    default:
                        break;
                }
            }

            return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);
        }

    }

}
