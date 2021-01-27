using Caliburn.Micro;
using ErogeHelper.Common.Extension;
using ErogeHelper.ViewModel;
using Serilog;
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

namespace ErogeHelper.Common.Service
{
    class SelectProcessService : ISelectProcessService
    {
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
                        Log.Warn(ex.Message);
                    }
                }
                foreach (var i in data.ToList())
                {
                    if (!tmpCollection.Contain(i))
                    {
                        data.Remove(i);
                    }
                }
            }).ConfigureAwait(false);
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
            "PaintStudio.View", "ShellExperienceHost", "commsapps", "Music.UI", "HxOutlook", "Maps"
        };
    }
}
