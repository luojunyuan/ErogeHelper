using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataModels;
using ErogeHelper.Model.Services.Interface;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Model.Services
{
    public class FilterProcessService : IFilterProcessService, IEnableLogger
    {
        public event Action<bool>? ShowAdminNeededTip;

        public IEnumerable<ProcessDataModel> Filter() =>
            Process.GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle != string.Empty)
                .Where(p =>
                {
                    string? fileName = null;
                    try
                    {
                        fileName = p.MainModule?.FileName;
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                    {
                        // Access Denied. 64bit -> 32bit module or need elevated permissions
                        this.Log().Info($"{p.MainWindowTitle} {ex.Message}");
                        ShowAdminNeededTip?.Invoke(true);
                        ShowAdminNeededTip = null;
                    }

                    return fileName is not null &&
                        !fileName.Contains(ConstantValues.UWPAppsTag) &&
                        !fileName.Contains(ConstantValues.WindowsPath) &&
                        p.Id != Environment.ProcessId;
                })
                .Select(p =>
                {
                    var fileName = p.MainModule?.FileName!;
                    var icon = PEIconToBitmapImage(fileName);
                    var descript = p.MainModule?.FileVersionInfo.FileDescription;
                    if (string.IsNullOrWhiteSpace(descript))
                        descript = p.MainWindowTitle;
                    return new ProcessDataModel(p, icon, descript);
                });

        private static BitmapImage PEIconToBitmapImage(string fullPath)
        {
            var result = new BitmapImage();

            Stream stream = new MemoryStream();

            var iconBitmap = (Icon.ExtractAssociatedIcon(fullPath) ?? throw new InvalidOperationException()).ToBitmap();
            iconBitmap.Save(stream, ImageFormat.Png);
            iconBitmap.Dispose();
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }
    }
}
