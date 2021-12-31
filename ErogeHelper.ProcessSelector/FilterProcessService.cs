using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ErogeHelper.ProcessSelector
{
    internal class FilterProcessService
    {
        private const string UwpAppsTag = "WindowsApps";
        private const string WindowsPath = @"C:\Windows\";
        private const string WindowsPathUpperCase = @"C:\WINDOWS\";
        private const int MaxTitleLength = 40;

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
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access is denied.
                    {
                        // need elevated permissions
                        Debug.WriteLine($"{p.MainWindowTitle} {ex.Message}");
                        ShowAdminNeededTip?.Invoke(true);
                        ShowAdminNeededTip = null;
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 299)
                    {
                        // 32bit -> 64bit
                        Debug.WriteLine($"{p.MainWindowTitle} {ex.Message}");
                    }

                    return fileName is not null &&
                        !fileName.Contains(UwpAppsTag) &&
                        !fileName.Contains(WindowsPath) &&
                        !fileName.Contains(WindowsPathUpperCase) &&
                        p.Id != Environment.ProcessId;
                })
                .Select(p =>
                {
                    var fileName = p.MainModule?.FileName!;
                    var icon = PeIconToBitmapImage(fileName);
                    var describe = p.MainModule?.FileVersionInfo.FileDescription ?? string.Empty;
                    var title = (p.MainWindowTitle.Length > MaxTitleLength && describe != string.Empty) ?
                        describe : p.MainWindowTitle;
                    return new ProcessDataModel(p, icon, describe, title);
                });

        private static BitmapImage PeIconToBitmapImage(string fullPath)
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
