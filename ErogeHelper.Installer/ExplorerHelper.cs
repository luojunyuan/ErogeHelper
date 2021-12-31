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
                Process.Start("explorer", dir);
            }
        }

        private class OpenedDirectoryGenerator
        {
            public OpenedDirectoryGenerator() => _ = EnumWindows(Report, 0);

            public List<string> Paths { get; private set; } = new();

            #region Win32 Apis
            private delegate bool CallBack(int hWnd, int y);

            [DllImport("user32.dll")]
            private static extern int EnumWindows(CallBack x, int y);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetWindowText(int hWnd, StringBuilder lPtrString, int nMaxCount);

            [DllImport("user32.dll")]
            private static extern int GetParent(int hWnd);

            [DllImport("user32.dll")]
            private static extern bool IsWindowVisible(int hWnd);

            [DllImport("user32.Dll", CharSet = CharSet.Unicode)]
            private static extern void GetClassName(IntPtr hWnd, StringBuilder s, int nMaxCount);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr child, string strClass, string? frmText);

            private static IntPtr FindWindowEx(IntPtr parent, string strClass)
                => FindWindowEx(parent, IntPtr.Zero, strClass, null);

            private static string GetFormClassName(IntPtr ptr)
            {
                var nameBuilder = new StringBuilder(255);
                GetClassName(ptr, nameBuilder, 255);
                return nameBuilder.ToString();
            }

            private static string GetFormTitle(IntPtr ptr)
            {
                var titleBuilder = new StringBuilder(255);
                _ = GetWindowText((int)ptr, titleBuilder, 255);
                return titleBuilder.ToString();
            }

            private bool Report(int hWnd, int lParam)
            {
                var pHWnd = GetParent(hWnd);
                if (pHWnd == 0 && IsWindowVisible(hWnd))
                {
                    var cabinetWClassIntPtr = new IntPtr(hWnd);
                    var cabinetWClassName = GetFormClassName(cabinetWClassIntPtr);
                    if (cabinetWClassName.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                    {
                        var workerWIntPtr = FindWindowEx(cabinetWClassIntPtr, "WorkerW");
                        var reBarWindow32IntPtr = FindWindowEx(workerWIntPtr, "ReBarWindow32");
                        var addressBandRootIntPtr = FindWindowEx(reBarWindow32IntPtr, "Address Band Root");
                        var msctlsProgress32IntPtr = FindWindowEx(addressBandRootIntPtr, "msctls_progress32");
                        var breadcrumbParentIntPtr = FindWindowEx(msctlsProgress32IntPtr, "Breadcrumb Parent");
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
