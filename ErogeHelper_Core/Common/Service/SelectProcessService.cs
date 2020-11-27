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
            data.Clear();

            await Task.Run(() =>
            {
                foreach (Process proc in ProcessEnumerable())
                {
                    try
                    {
                        data.Add(new ProcComboboxItem()
                        {
                            proc = proc,
                            Title = proc.MainWindowTitle,
                            Icon = Utils.PEIcon2BitmapImage(proc.GetMainModuleFileName())
                        });
                    }
                    catch(Win32Exception ex)
                    {
                        // Access Denied. it's doesn't matter?
                        log.Error(ex.Message);
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

        private List<string> uselessProcess = new List<string> 
        { 
            "TextInputHost", "ApplicationFrameHost", "Calculator", "Video.UI", "WinStore.App", "SystemSettings",
        };
    }
}
