using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ErogeHelper.Installer
{
    internal static class ExplorerHelper
    {
        public static List<string> GetOpenedDirectories() => new OpenedDirectoryGenerator().Paths;

        public static void KillExplorer()
        {
            var processes = Process.GetProcessesByName("explorer");

            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        public static void OpenDirectories(List<string> directories)
        {
            foreach (var dir in directories)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer",
                    Arguments = dir
                });
            }
        }

        private class OpenedDirectoryGenerator
        {
            public OpenedDirectoryGenerator() => _ = EnumWindows(Report, 0);

            public List<string> Paths { get; private set; } = new();

            #region Win32 Apis
            private delegate bool CallBack(int hwnd, int y);

            [DllImport("user32.dll")]
            private static extern int EnumWindows(CallBack x, int y);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetWindowText(int hwnd, StringBuilder lptrString, int nMaxCount);

            [DllImport("user32.dll")]
            private static extern int GetParent(int hwnd);

            [DllImport("user32.dll")]
            private static extern bool IsWindowVisible(int hwnd);

            [DllImport("user32.Dll", CharSet = CharSet.Unicode)]
            private static extern void GetClassName(IntPtr hwnd, StringBuilder s, int nMaxCount);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childe, string strclass, string? FrmText);

            private static IntPtr FindWindowEx(IntPtr parent, string strClass)
                => FindWindowEx(parent, IntPtr.Zero, strClass, null);

            private static string GetFormClassName(IntPtr ptr)
            {
                var nameBiulder = new StringBuilder(255);
                GetClassName(ptr, nameBiulder, 255);
                return nameBiulder.ToString();
            }

            private static string GetFormTitle(IntPtr ptr)
            {
                var titleBiulder = new StringBuilder(255);
                _ = GetWindowText((int)ptr, titleBiulder, 255);
                return titleBiulder.ToString();
            }

            private bool Report(int hwnd, int lParam)
            {
                var pHwnd = GetParent(hwnd);
                if (pHwnd == 0 && IsWindowVisible(hwnd))
                {
                    var cabinetWClassIntPtr = new IntPtr(hwnd);
                    var cabinetWClassName = GetFormClassName(cabinetWClassIntPtr);
                    if (cabinetWClassName.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                    {
                        var workerWIntPtr = FindWindowEx(cabinetWClassIntPtr, "WorkerW");
                        var reBarWindow32IntPtr = FindWindowEx(workerWIntPtr, "ReBarWindow32");
                        var addressBandRootIntPtr = FindWindowEx(reBarWindow32IntPtr, "Address Band Root");
                        var msctls_progress32IntPtr = FindWindowEx(addressBandRootIntPtr, "msctls_progress32");
                        var breadcrumbParentIntPtr = FindWindowEx(msctls_progress32IntPtr, "Breadcrumb Parent");
                        var toolbarWindow32IntPtr = FindWindowEx(breadcrumbParentIntPtr, "ToolbarWindow32");

                        var title = GetFormTitle(toolbarWindow32IntPtr);

                        var index = title.IndexOf(':') + 1;
                        Paths.Add(title[index..]);
                    }
                }
                return true;
            }
            #endregion
        }
    }
}
