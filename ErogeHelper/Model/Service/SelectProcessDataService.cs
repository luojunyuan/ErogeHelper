using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Comparer;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class SelectProcessDataService : ISelectProcessDataService, IDisposable
    {
        public async Task RefreshBindableProcComboBoxAsync(BindableCollection<ProcComboBoxItem> refData) =>
            await Task.Run(() =>
            {
                BindableCollection<ProcComboBoxItem> tmpCollection = new();

                foreach (Process proc in ProcessEnumerable())
                {
                    try
                    {
                        var item = new ProcComboBoxItem
                        {
                            Proc = proc,
                            Title = proc.MainWindowTitle,
                            Icon = Utils.PeIcon2BitmapImage(proc.MainModule?.FileName ?? string.Empty)
                        };
                        tmpCollection.Add(item);

                        if (refData.Contains(item, new ProcComboBoxItemComparer()))
                            continue;

                        refData.Add(item);
                    }
                    catch (Win32Exception ex)
                    {
                        // Access Denied. 64bit -> 32bit module
                        Log.Debug(ex.Message);
                    }
                }

                var redundantItems = refData.ToList()
                    .Where(i => !tmpCollection.Contains(i, new ProcComboBoxItemComparer()));
                foreach (var i in redundantItems)
                {
                    refData.Remove(i);
                }
            }).ConfigureAwait(false);

        private IEnumerable<Process> ProcessEnumerable() =>
            Process.GetProcesses()
                .Where(proc => 
                    proc.MainWindowHandle != IntPtr.Zero
                    && proc.MainWindowTitle != string.Empty
                    && !_uselessProcess.Contains(proc.ProcessName));

        private readonly IEnumerable<string> _uselessProcess = new []
        {
            "TextInputHost", "ApplicationFrameHost", "Calculator", "Video.UI", "WinStore.App", "SystemSettings",
            "PaintStudio.View", "ShellExperienceHost", "commsapps", "Music.UI", "HxOutlook", "Maps", "WhiteboardWRT",
            "PeopleApp", "RtkUWP"
        };

        public void Dispose() => Log.Debug($"{nameof(SelectProcessDataService)}.Dispose()");
    }
}