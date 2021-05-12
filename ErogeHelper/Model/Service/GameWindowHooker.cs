using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Windows.Sdk;
using Splat;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ErogeHelper.Common;
using ErogeHelper.Model.DataService.Interface;
using ErogeHelper.Common.Contract;

namespace ErogeHelper.Model.Service
{
    class GameWindowHooker : IGameWindowHooker, IEnableLogger
    {
        public event EventHandler<GameWindowPositionEventArgs>? GamePosChanged;
        
        public void SetGameWindowHook(Process process)
        {
            _gameProc = process;

            _winEventDelegate = WinEventCallback;
            _gcSafetyHandle = GCHandle.Alloc(_winEventDelegate);

            _gameProc.EnableRaisingEvents = true;
            _gameProc.Exited += ApplicationExit;

            ResetWindowHandler();
        }

        public void ResetWindowHandler()
        {
            if (_realWindowHandle != IntPtr.Zero)
            { 
                PInvoke.GetWindowRect(_realWindowHandle, out var windowRect);
                if ($"{windowRect.left} {windowRect.top} {windowRect.right} {windowRect.bottom}"
                    .Equals(MinimizedPosition))
                {
                    // Ignore minimized window situation
                    this.Log().Debug("window minimized");
                    return;
                }
            }

            HWND handle = FindRealHandle();

            if (handle != IntPtr.Zero)
            { 
                _realWindowHandle = handle;
                SetWindowHandler();
            }
            else
            {
                // gameHWnd still be last handle
                throw new InvalidOperationException("Not found game window");
            }
            UpdateLocation();
        }

        public void InvokeUpdatePosition() => UpdateLocation();

        private WINEVENTPROC? _winEventDelegate;
        private HWINEVENTHOOK _hWinEventHook; // return type
        private GCHandle _gcSafetyHandle;
        private Process _gameProc = new();
        private HWND _realWindowHandle;

        private static readonly HWND IntPtrZero = new ();
        private const string MinimizedPosition = "-32000 -32000 -31840 -31972";
        private static readonly GameWindowPositionEventArgs HiddenPos = new()
        {
            ClientArea = new Thickness(),
            Left = -32000,
            Top = -32000,
            Height = 0,
            Width = 0,
        };

        private unsafe void SetWindowHandler()
        {
            this.Log().Debug($"Set handle to 0x{Convert.ToString(_realWindowHandle.Value, 16).ToUpper()} " +
                     $"Title: {_gameProc.MainWindowTitle}");
            var gameUIThreadId = PInvoke.GetWindowThreadProcessId(_realWindowHandle);

            // Hook must register in message loop thread
            Dispatcher.InvokeAsync(
                () => _hWinEventHook = WinEventHookRange(
                    SWEHEvents.EventObjectFocus,
                    SWEHEvents.EventObjectLocationChange,
                    _winEventDelegate ?? throw new ArgumentNullException(nameof(_winEventDelegate)),
                    (uint)_gameProc.Id,
                    gameUIThreadId)
            );
        }

        private void WinEventCallback(HWINEVENTHOOK hWinEventHook,
                                      uint eventType,
                                      HWND hWnd,
                                      int idObject,
                                      int idChild,
                                      uint dwEventThread,
                                      uint dwmsEventTime)
        {
            // When game window get focus
            if (hWnd.Equals(_realWindowHandle) &&
                eventType == (uint)SWEHEvents.EventObjectFocus)
            {
                //UpdateLocation();
            }

            // When game window location changed
            if (hWnd.Equals(_realWindowHandle) &&
                eventType == (uint)SWEHEvents.EventObjectLocationChange &&
                idObject == SWEH_CHILDID_SELF)
            {
                UpdateLocation();
            }
        }

        private void UpdateLocation()
        {
            PInvoke.GetWindowRect(_realWindowHandle, out var rectWindow);
            PInvoke.GetClientRect(_realWindowHandle, out var rectClient);

            var width = rectWindow.right - rectWindow.left;  // equal rectClient.Right + shadow*2
            var height = rectWindow.bottom - rectWindow.top; // equal rectClient.Bottom + shadow + title

            var winShadow = (width - rectClient.right) / 2;

            var wholeHeight = rectWindow.bottom - rectWindow.top;
            var winTitleHeight = wholeHeight - rectClient.bottom - winShadow;

            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            GamePosChanged?.Invoke(this, new GameWindowPositionEventArgs
            {
                Height = height,
                Width = width,
                Left = rectWindow.left,
                Top = rectWindow.top,
                ClientArea = clientArea
            });
        }

        private HWND FindRealHandle()
        {
            List<HWND> handleList = GetRootWindowsOfProcess(_gameProc.Id).ToList();
            this.Log().Debug($"{handleList.Count} handles found");

            foreach (var handle in handleList)
            {
                PInvoke.GetClientRect(handle, out var clientRect);
                if (clientRect.bottom > ConstantValues.GoodWindowSize && clientRect.right > ConstantValues.GoodWindowSize)
                {
                    return handle;
                }
            }
            this.Log().Debug("Find failed");

            return IntPtrZero;
        }

        private delegate bool Win32Callback(HWND hwnd, IntPtr lParam);

        #region SetWinEventHook Warpper

        private static HWINEVENTHOOK WinEventHookRange(SWEHEvents eventFrom, SWEHEvents eventTo,
                                                WINEVENTPROC @delegate,
                                                uint idProcess, 
                                                uint idThread)
        {
            return PInvoke.SetWinEventHook((uint)eventFrom, (uint)eventTo, IntPtr.Zero, @delegate, idProcess, idThread,
                                            (uint)WinEventHookInternalFlags);
        }

        private static HWINEVENTHOOK WinEventHookOne(SWEHEvents @event, 
                                              WINEVENTPROC @delegate, 
                                              uint idProcess, 
                                              uint idThread)
        {
            return PInvoke.SetWinEventHook((uint)@event, (uint)@event, IntPtr.Zero, @delegate, idProcess, idThread,
                                            (uint)WinEventHookInternalFlags);
        }

        private static readonly SWEH_dwFlags WinEventHookInternalFlags = SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

        #endregion

        #region Enumerate Handle By Pid

        private unsafe IEnumerable<HWND> GetRootWindowsOfProcess(int pid)
        {
            IEnumerable<HWND> rootWindows = GetChildWindows(IntPtrZero);
            List<HWND> dsProcRootWindows = new();
            foreach (var hWnd in rootWindows)
            {
                uint lpdwProcessId = 0;
                _ = PInvoke.GetWindowThreadProcessId(hWnd, &lpdwProcessId);
                if ((int)lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        private IEnumerable<HWND> GetChildWindows(HWND parent)
        {
            List<HWND> result = new();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                WNDENUMPROC childProc = EnumWindow;
                PInvoke.EnumChildWindows(parent, childProc, (LPARAM)GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private BOOL EnumWindow(HWND handle, LPARAM pointer)
        {
            var gch = GCHandle.FromIntPtr(pointer);
            if (gch.Target is not List<HWND> list)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<HWND>");
            }
            list.Add(handle);
            return true;
        }

        #endregion

        private void ApplicationExit(object? sender, EventArgs e)
        {
            this.Log().Debug("Detected game quit event");
            GamePosChanged?.Invoke(this, HiddenPos);
            _gcSafetyHandle.Free();
            PInvoke.UnhookWinEvent(_hWinEventHook);

            Dispatcher.InvokeShutdown();
        }

        // For running in the unit test
        private static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        #region Enums

        private const int SWEH_CHILDID_SELF = 0;
        
        private enum SWEHEvents : uint
        {
            EventMin = 0x00000001,
            EventMax = 0x7FFFFFFF,
            EventSystemSound = EventMin,
            EventSystemAlert = 0x0002,
            EventSystemForeground = 0x0003,
            EventSystemMenuStart = 0x0004,
            EventSystemMenuEnd = 0x0005,
            EventSystemMenuPopupStart = 0x0006,
            EventSystemMenuPopupEnd = 0x0007,
            EventSystemCaptureStart = 0x0008,
            EventSystemCaptureEnd = 0x0009,
            EventSystemMoveSizeStart = 0x000A,
            EventSystemMoveSizeEnd = 0x000B,
            EventSystemContextHelpStart = 0x000C,
            EventSystemContextHelpEnd = 0x000D,
            EventSystemDragDropStart = 0x000E,
            EventSystemDragDropEnd = 0x000F,
            EventSystemDialogStart = 0x0010,
            EventSystemDialogEnd = 0x0011,
            EventSystemScrollingStart = 0x0012,
            EventSystemScrollingEnd = 0x0013,
            EventSystemSwitchStart = 0x0014,
            EventSystemSwitchEnd = 0x0015,
            EventSystemMinimizeStart = 0x0016,
            EventSystemMinimizeEnd = 0x0017,
            EventSystemDesktopSwitch = 0x0020,
            EventSystemEnd = 0x00FF,
            EventOemDefinedStart = 0x0101,
            EventOemDefinedEnd = 0x01FF,
            EventUiaEventIdStart = 0x4E00,
            EventUiaEventIdEnd = 0x4EFF,
            EventUiaPropIdStart = 0x7500,
            EventUiaPropIdEnd = 0x75FF,
            EventConsoleCaret = 0x4001,
            EventConsoleUpdateRegion = 0x4002,
            EventConsoleUpdateSimple = 0x4003,
            EventConsoleUpdateScroll = 0x4004,
            EventConsoleLayout = 0x4005,
            EventConsoleStartApplication = 0x4006,
            EventConsoleEndApplication = 0x4007,
            EventConsoleEnd = 0x40FF,
            EventObjectCreate = 0x8000,               // hWnd ID idChild is created item
            EventObjectDestroy = 0x8001,              // hWnd ID idChild is destroyed item
            EventObjectShow = 0x8002,                 // hWnd ID idChild is shown item
            EventObjectHide = 0x8003,                 // hWnd ID idChild is hidden item
            EventObjectReorder = 0x8004,              // hWnd ID idChild is parent of z-ordering children
            EventObjectFocus = 0x8005,                // * hWnd ID idChild is focused item
            EventObjectSelection = 0x8006,            // hWnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOWif complex
            EventObjectSelectionAdd = 0x8007,         // hWnd ID idChild is item added
            EventObjectSelectionRemove = 0x8008,      // hWnd ID idChild is item removed
            EventObjectSelectionWithin = 0x8009,      // hWnd ID idChild is parent of changed selected items
            EventObjectStateChange = 0x800A,          // hWnd ID idChild is item w/ state change
            EventObjectLocationChange = 0x800B,       // * hWnd ID idChild is moved/sized item
            EventObjectNameChange = 0x800C,           // hWnd ID idChild is item w/ name change
            EventObjectDescriptionChange = 0x800D,    // hWnd ID idChild is item w/ desc change
            EventObjectValueChange = 0x800E,          // hWnd ID idChild is item w/ value change
            EventObjectParentChange = 0x800F,         // hWnd ID idChild is item w/ new parent
            EventObjectHelpChange = 0x8010,           // hWnd ID idChild is item w/ help change
            EventObjectDefactionChange = 0x8011,      // hWnd ID idChild is item w/ def action change
            EventObjectAcceleratorChange = 0x8012,    // hWnd ID idChild is item w/ keybd accel change
            EventObjectInvoked = 0x8013,              // hWnd ID idChild is item invoked
            EventObjectTextSelectionChanged = 0x8014, // hWnd ID idChild is item w? test selection change
            EventObjectContentScrolled = 0x8015,
            EventSystemArrangementPreview = 0x8016,
            EventObjectEnd = 0x80FF,
            EventAiaStart = 0xA000,
            EventAiaEnd = 0xAFFF
        }

        private enum SWEH_dwFlags : uint
        {
            WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
            WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
            WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
            WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
        }

        #endregion
    }
}
