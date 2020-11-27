using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.Common.Service
{
    class SelectProcessService : ISelectProcessService
    {
        public async Task GetProcessListAsync(BindableCollection<string> data)
        {
            data.Clear();

            await Task.Run(() =>
            {
                foreach (var proc in ProcessEnumerable())
                {
                    data.Add(proc.MainWindowTitle);
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
