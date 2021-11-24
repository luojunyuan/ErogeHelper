using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ProcessSelector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private const string UniqueEventName = "{a5f52aac-d734-4ff2-bbf2-426025628837}";

        public App()
        {
            Current.DispatcherUnhandledException += (_, args) =>
            {
                var ex = args.Exception;
                MessageBox.Show(ex.ToString());
            };

            SingleInstanceWatcher();
            var currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
            Directory.SetCurrentDirectory(currentDirectory ??
                                          throw new ArgumentNullException(nameof(currentDirectory)));
        }

        // http://stackoverflow.com/a/23730146/1644202"
        private static void SingleInstanceWatcher()
        {
            if (EventWaitHandle.TryOpenExisting(UniqueEventName, out var eventWaitHandle))
            {
                eventWaitHandle.Set();
                Current.Shutdown();
            }
            else
            {
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
                Task.Factory.StartNew(() =>
                {
                    while (eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.Invoke(() =>
                        {
                            if (Current.MainWindow is not null)
                            {
                                var mainWindow = Current.MainWindow;

                                if (mainWindow.WindowState == WindowState.Minimized ||
                                    mainWindow.Visibility != Visibility.Visible)
                                {
                                    mainWindow.Show();
                                    mainWindow.WindowState = WindowState.Normal;
                                }

                                mainWindow.Activate();
                                mainWindow.Topmost = true;
                                mainWindow.Topmost = false;
                                mainWindow.Focus();
                            }
                        });
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }
    }
}
