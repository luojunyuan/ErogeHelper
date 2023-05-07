using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ErogeHelper.KeyMapping
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parentPid = int.Parse(args[0]);
            var gameWindowHandle = (IntPtr)int.Parse(args[1]);
            var parent = Process.GetProcessById(parentPid);
            parent.EnableRaisingEvents = true;
            parent.Exited += (s, e) =>
            {
                KeyboardHooker.UnInstall();
                Environment.Exit(0);
            };

            KeyboardHooker.Install(gameWindowHandle);

            while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(msg);
                DispatchMessage(msg);
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetMessage(out IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        static extern bool TranslateMessage(in IntPtr lpMsg);
        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage(in IntPtr lpMsg);
    }
}
