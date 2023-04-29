using ErogeHelper.AssistiveTouch.NativeMethods;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
}
