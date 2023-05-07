using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ErogeHelper.Magnifier
{
    internal class GameWindowHooker : IDisposable
    {
        public EventHandler<Point> WindowPositionDeltaChanged;

        private readonly IntPtr _windowsEventHook;

        private readonly GCHandle _gcSafetyHandle;

        private readonly IntPtr _windowHandle;

        public GameWindowHooker(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;

            var targetThreadId = GetWindowThreadProcessId(windowHandle, out var pid);

            WinEventProc winEventDelegate = WinEventCallback;
            _gcSafetyHandle = GCHandle.Alloc(winEventDelegate);

            _windowsEventHook = SetWinEventHook(
                 EventObjectLocationChange, EventObjectLocationChange,
                 IntPtr.Zero, winEventDelegate, pid, targetThreadId,
                 WinEventHookInternalFlags);

            GetWindowRect(windowHandle, out var rect);
            _windowLocation = new Point(rect.Left, rect.Top);
        }

        private const WINEVENT WinEventHookInternalFlags = WINEVENT.WINEVENT_INCONTEXT | WINEVENT.WINEVENT_SKIPOWNPROCESS;
        private const uint EventObjectLocationChange = 0x800B;
        private const long SWEH_CHILDID_SELF = 0;
        private const int OBJID_WINDOW = 0;

        private Point _windowLocation;

        /// <summary>
        /// Running in UI thread
        /// </summary>
        private void WinEventCallback(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hWnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            if (eventType == EventObjectLocationChange &&
                hWnd == _windowHandle &&
                idObject == OBJID_WINDOW && idChild == SWEH_CHILDID_SELF)
            {
                GetWindowRect(hWnd, out var rect);
                var newPos = new Point(rect.Left, rect.Top);
                if (newPos != _windowLocation)
                {
                    WindowPositionDeltaChanged.Invoke(this, new Point(newPos.X - _windowLocation.X, newPos.Y - _windowLocation.Y));
                    _windowLocation = newPos;
                }

            }
        }

        public void Dispose()
        {
            _gcSafetyHandle.Free();
            UnhookWinEvent(_windowsEventHook);
        }

        [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc pfnWinEventProc, uint idProcess, uint idThread, WINEVENT dwFlags);

        public delegate void WinEventProc(IntPtr hWinEventHook, uint winEvent, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        [Flags]
        public enum WINEVENT
        {
            WINEVENT_INCONTEXT = 0,
            WINEVENT_OUTOFCONTEXT = 1,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_SKIPOWNTHREAD = 4,
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    }
}
