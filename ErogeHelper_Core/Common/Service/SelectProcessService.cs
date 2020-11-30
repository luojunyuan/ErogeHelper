using Caliburn.Micro;
using ErogeHelper_Core.Common.Extension;
using ErogeHelper_Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ErogeHelper_Core.Common.Service
{
    class SelectProcessService : ISelectProcessService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SelectProcessService));

        public async Task GetProcessListAsync(BindableCollection<ProcComboboxItem> data)
        {
            await Task.Run(() =>
            {
                BindableCollection<ProcComboboxItem> tmpCollection = new BindableCollection<ProcComboboxItem>();

                foreach (Process proc in ProcessEnumerable())
                {
                    try
                    {
                        var item = new ProcComboboxItem()
                        {
                            proc = proc,
                            Title = proc.MainWindowTitle,
                            Icon = Utils.PEIcon2BitmapImage(proc.GetMainModuleFileName())
                        };
                        tmpCollection.Add(item);
                        if (data.Contain(item))
                            continue;
                        else
                            data.Add(item);
                    }
                    catch(Win32Exception ex)
                    {
                        // Casue by `GetMainModuleFileName()`
                        // Access Denied. 32bit -> 64bit module
                        log.Error(ex.Message);
                    }
                }
                foreach (var i in data.ToList())
                {
                    if (!tmpCollection.Contain(i))
                    {
                        data.Remove(i);
                    }
                }
            });
        }

        private IEnumerable<Process> ProcessEnumerable()
        {
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.MainWindowHandle != IntPtr.Zero && !string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                {
                    if (uselessProcess.Contains(proc.ProcessName)) 
                        continue;

                    yield return proc;
                }
            }
        }

        private readonly List<string> uselessProcess = new List<string> 
        { 
            "TextInputHost", "ApplicationFrameHost", "Calculator", "Video.UI", "WinStore.App", "SystemSettings",
            "PaintStudio.View", "ShellExperienceHost"
        };
    }
}
