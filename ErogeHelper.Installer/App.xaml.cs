using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            SingleInstanceWatcher();
            var currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
            Directory.SetCurrentDirectory(currentDirectory ??
                                          throw new ArgumentNullException(nameof(currentDirectory)));
        }

        private const string UniqueEventName = "{2d0ccd54-f861-46be-9804-43aff3775111}";
        private EventWaitHandle _eventWaitHandle = null!;

        private void SingleInstanceWatcher()
        {
            try
            {
                _eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                _eventWaitHandle.Set();
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            new Task(() =>
            {
                while (_eventWaitHandle.WaitOne())
                {
                    Current.Dispatcher.Invoke(() =>
                    {
                        if (Current.MainWindow is not null)
                        {
                            var mainWindow = Current.MainWindow;

                            if (mainWindow.WindowState == WindowState.Minimized || mainWindow.Visibility != Visibility.Visible)
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
            })
            .Start();
        }
    }
}
