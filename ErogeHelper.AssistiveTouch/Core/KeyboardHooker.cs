using ErogeHelper.AssistiveTouch.NativeMethods;
using System.ComponentModel;
using System.Runtime.InteropServices;
using WindowsInput.Events;
using WindowsInput.Native;

namespace ErogeHelper.AssistiveTouch.Core
{
    internal class KeyboardHooker
    {
        private static IntPtr _hookId;
        private static IntPtr _gameWindowHandle;

        public static void Install(IntPtr gameWindowHandle)
        {
            _gameWindowHandle = gameWindowHandle;
            var moduleHandle = Kernel32.GetModuleHandle(); // get current exe instant handle

            _hookId = User32.SetWindowsHookEx(User32.HookType.WH_KEYBOARD_LL, Hook, moduleHandle, 0); // tid 0 set global hook
            if (_hookId == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void UnInstall() => User32.UnhookWindowsHookEx(_hookId);

        private static IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);

            var obj = Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if (obj is not KBDLLHOOKSTRUCT info)
                return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);

            if (info.vkCode == (uint)Config.MappingKey && User32.GetForegroundWindow() == _gameWindowHandle)
            {
                const int WM_KEYUP = 0x0101;
                const int KEYBOARDMANAGER_SINGLEKEY_FLAG = 0x11;
                var keyEventList = new INPUT[1];
                if ((int)wParam == WM_KEYUP)
                {
                    SetKeyEvent(keyEventList, KeyCode.Enter, KeyboardFlag.KeyUp, (IntPtr)KEYBOARDMANAGER_SINGLEKEY_FLAG);
                }
                else
                {
                    SetKeyEvent(keyEventList, KeyCode.Enter, KeyboardFlag.KeyDown, (IntPtr)KEYBOARDMANAGER_SINGLEKEY_FLAG);
                }

                SendInput(1, keyEventList, Marshal.SizeOf(keyEventList[0]));
                return new IntPtr(1);
            }

            return User32.CallNextHookEx(_hookId!, nCode, wParam, lParam);
        }

        static void SetKeyEvent(INPUT[] keyEventArray, KeyCode keyCode, KeyboardFlag flags, IntPtr extraInfo)
        {
            keyEventArray[0].Type = INPUTTYPE.Keyboard;
            keyEventArray[0].Data.Keyboard.KeyCode = keyCode;
            keyEventArray[0].Data.Keyboard.Flags = flags;
            keyEventArray[0].Data.Keyboard.ExtraInfo = extraInfo;

            const int MAPVK_VK_TO_VSC = 0;
            keyEventArray[0].Data.Keyboard.ScanCode = (ushort)MapVirtualKey((int)keyCode, MAPVK_VK_TO_VSC);
        }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // 仮想キーコードをスキャンコードに変換
        [DllImport("user32.dll")]
        private extern static int MapVirtualKey(int wCode, int wMapType);

        [Flags]
        private enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        [StructLayout(LayoutKind.Sequential)]
        private class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
    }
}
