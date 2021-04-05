using Caliburn.Micro;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        public static BitmapImage PeIcon2BitmapImage(string fullPath)
        {
            var result = new BitmapImage();

            if (fullPath == string.Empty)
                return result;

            Stream stream = new MemoryStream();

            var iconBitmap = Icon.ExtractAssociatedIcon(fullPath)!.ToBitmap();
            iconBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            iconBitmap.Dispose();
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /// <summary>
        /// Get all processes of the game (timeout 20s)
        /// </summary>
        /// <param name="friendlyName">aka <see cref="Process.ProcessName"/></param>
        /// <returns>if process with hWnd found, give all back, other wise return blank list</returns>
        public static (IEnumerable<Process>, Process) ProcessCollect(string friendlyName)
        {
            const int timeoutSeconds = 20;
            var spendTime = new Stopwatch();
            spendTime.Start();
            Process? mainProcess = null;
            var procList = new List<Process>();
            while (mainProcess is null && spendTime.Elapsed.TotalSeconds < timeoutSeconds)
            {
                procList.Clear();
                procList.AddRange(Process.GetProcessesByName(friendlyName));
                procList.AddRange(Process.GetProcessesByName(friendlyName + ".log"));
                procList.AddRange(Process.GetProcessesByName("main.bin"));

                // 进程找完却没有得到hWnd的可能也是存在的，所以以带hWnd的进程为主
                mainProcess = procList.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            spendTime.Stop();

            // log 👀
            if (mainProcess is null)
            {
                Log.Info("Timeout! Find MainWindowHandle Failed");
            }
            else
            {
                Log.Info($"{procList.Count} Process(es) and window handle " +
                         $"0x{Convert.ToString(mainProcess.MainWindowHandle.ToInt64(), 16).ToUpper()} Found. " +
                         $"Spend time {spendTime.Elapsed.TotalSeconds:0.00}s");
            }

            return mainProcess is null ? (new List<Process>(), new Process()) : (procList, mainProcess);
        }

        public static void HideWindowInAltTab(Window window)
        {
            var windowInterop = new WindowInteropHelper(window);
            var exStyle = NativeMethods.GetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE);
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(windowInterop.Handle, NativeMethods.GWL_EXSTYLE, exStyle);
        }

        /// <summary>
        /// Get MD5 hash by file
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <returns>Upper case string</returns>
        public static string GetFileMd5(string filePath)
        {
            FileStream file = File.OpenRead(filePath);
            var md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            foreach (var byteItem in retVal)
            {
                sb.Append(byteItem.ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// Update the database
        /// </summary>
        public static void UpdateEhDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }

        /// <summary>
        /// Wrapper no need characters with |~S~| |~E~|
        /// </summary>
        /// <param name="sourceInput"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string TextEvaluateWithRegExp(string sourceInput, string expr)
        {
            const string begin = "|~S~|";
            const string end = "|~E~|";

            if (expr == string.Empty)
                return sourceInput;

            if (expr[^1] == '|')
                return sourceInput;

            string wrapperText = sourceInput;

            var instant = new Regex(expr);
            var collect = instant.Matches(sourceInput);
            foreach (Match match in collect)
            {
                var beginPos = wrapperText.LastIndexOf(end, StringComparison.Ordinal);
                wrapperText = instant.Replace(
                    wrapperText,
                    begin + match + end,
                    1,
                    beginPos == -1 ? 0 : beginPos + 5);
            }
            return wrapperText;

        }

        public static Dictionary<string, string> GetGameNamesByProcess(Process proc)
        {
            var fullPath = proc.MainModule?.FileName ?? string.Empty;
            var fullDir = Path.GetDirectoryName(fullPath) ?? string.Empty;

            var file = Path.GetFileName(fullPath);
            var dir = Path.GetFileName(fullDir);
            var title = proc.MainWindowTitle;
            var fileWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);

            return new Dictionary<string, string>
            {
                { "File", file},
                { "Dir", dir},
                { "Title", title},
                { "FileNoExt", fileWithoutExtension },
            };
        }

        public static Dictionary<string, string> GetGameNamesByPath(string absolutePath)
        {
            var file = Path.GetFileName(absolutePath);
            var fullDir = Path.GetDirectoryName(absolutePath) ?? string.Empty;
            var dir = Path.GetFileName(fullDir);
            var fileWithoutExtension = Path.GetFileNameWithoutExtension(absolutePath);

            return new Dictionary<string, string>
            {
                { "File", file },
                { "Dir", dir },
                { "FileNoExt", fileWithoutExtension },
            };
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 
        /// 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentioned, I will use the calling
        /// assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            if (pathInApplication[0] == '/')
                pathInApplication = pathInApplication[1..];

            try
            {
                var uri = new Uri(
                    @"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication,
                    UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch (UriFormatException ex)
            {
                // May running in unit test
                Log.Warn(ex);
                return new BitmapImage();
            }
        }

        public static void OpenUrl(string urlLink) => new Process
        {
            StartInfo = new ProcessStartInfo(urlLink)
            {
                UseShellExecute = true
            }
        }.Start();

        public static BindableCollection<SingleTextItem> BindableTextMaker(
            string sentence,
            Func<string, IEnumerable<MeCabWord>> callback,
            TextTemplateType templateType)
        {
            BindableCollection<SingleTextItem> collect = new();
            // 必须在与 DependencyObject 相同的 Thread 上创建 DependencySource
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var word in callback(sentence))
                {
                    collect.Add(new SingleTextItem(
                        word.Kana,
                        word.Word,
                        templateType,
                        word.PartOfSpeech.ToColor()));
                }
            });
            return collect;
        }
    }
}