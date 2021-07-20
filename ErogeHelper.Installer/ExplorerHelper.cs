using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Installer
{
    static class ExplorerHelper
    {
        public static IEnumerable<string> GetOpenedDirectories()
        {
            var enumerator = new OpenedDirectoryEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static void KillExplorer()
        {
            var processes = Process.GetProcessesByName("explorer");

            foreach (Process process in processes)
            {
                process.Kill();
            }
        }
        public static void OpenDirectories(IEnumerable<string> directories)
        {
            foreach (string dir in directories)
            {
                _ = Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer",
                    Arguments = dir
                });
            }
        }

        [SuppressMessage("Globalization", "CA2101:指定对 P/Invoke 字符串参数进行封送处理", Justification = "<挂起>")]
        private class OpenedDirectoryEnumerator : IEnumerator<string>
        {
            private readonly EventWaitHandle _eventWaitHandle = new(false, EventResetMode.AutoReset);
            private readonly Task? _enumeratingTask;
            private string? _current;
            private bool _completed;

            public OpenedDirectoryEnumerator()
            {
                _enumeratingTask = Enumerate();
            }

            public string Current => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _eventWaitHandle.Dispose();
            }

            public bool MoveNext()
            {
                return !_completed && _eventWaitHandle.WaitOne();
            }

            public void Reset() => throw new NotSupportedException();

            [DllImport("user32.dll")]
            private static extern int EnumWindows(CallBack lpEnumFunc, int lPararm);

            [DllImport("user32.dll")]
            private static extern int GetParent(int hwnd);

            [DllImport("user32.dll")]
            private static extern int GetWindowText(int hwnd, StringBuilder lptrString, int nMaxCount);

            [DllImport("user32.dll")]
            private static extern bool IsWindowVisible(int hwnd);

            [DllImport("user32.dll")]
            private static extern int GetClassName(IntPtr hwnd, StringBuilder s, int nMaxCount);

            [DllImport("user32.dll")]
            private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr child, string strclass, string? frmText);

            private Task Enumerate() => Task.Run(() =>
            {
                _ = EnumWindows(Handle, 0).CheckError();
                _completed = true;
            });

            private bool Handle(int hwnd, int lParam)
            {
                string? path = GetPath(hwnd);

                if (path is not null)
                {
                    _current = path;
                    while (!_eventWaitHandle.Set()) { }
                }

                return true;
            }

            private static string GetFormClassName(IntPtr ptr)
            {
                var builder = new StringBuilder(255);
                _ = GetClassName(ptr, builder, 255).CheckError();

                return builder.ToString();
            }

            private static string GetFormTitle(IntPtr ptr)
            {
                var builder = new StringBuilder(255);
                _ = GetWindowText((int)ptr, builder, 255).CheckError();

                return builder.ToString();
            }

            private static string? GetPath(int hwnd)
            {
                int pHwnd = GetParent(hwnd);

                if (pHwnd == 0 && IsWindowVisible(hwnd))
                {
                    var cabinetWClassIntPtr = new IntPtr(hwnd);
                    string cabinetWClassName = GetFormClassName(cabinetWClassIntPtr);

                    if (cabinetWClassName.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                    {
                        var workerWIntPtr = FindWindowEx(cabinetWClassIntPtr, "WorkerW");
                        var reBarWindow32IntPtr = FindWindowEx(workerWIntPtr, "ReBarWindow32");
                        var addressBandRootIntPtr = FindWindowEx(reBarWindow32IntPtr, "Address Band Root");
                        var msctls_progress32IntPtr = FindWindowEx(addressBandRootIntPtr, "msctls_progress32");
                        var breadcrumbParentIntPtr = FindWindowEx(msctls_progress32IntPtr, "Breadcrumb Parent");
                        var toolbarWindow32IntPtr = FindWindowEx(breadcrumbParentIntPtr, "ToolbarWindow32");

                        string title = GetFormTitle(toolbarWindow32IntPtr);
                        int index = title.IndexOf(':') + 1;

                        return title[index..].Trim();
                    }
                }

                return null;
            }

            private static IntPtr FindWindowEx(IntPtr parent, string strClass)
                => FindWindowEx(parent, IntPtr.Zero, strClass, null);

            private delegate bool CallBack(int hwnd, int lParam);
        }
    }
}
