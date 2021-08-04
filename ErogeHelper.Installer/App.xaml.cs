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
            // initiate it. Call it first.
            SingleInstanceWatcher();
        }

        private const string UniqueEventName = "{2d0ccd54-f861-46be-9804-43aff3775111}";
        private EventWaitHandle _eventWaitHandle = null!;

        /// <summary>prevent a second instance and signal it to bring its MainWindow to foreground</summary>
        /// <seealso cref="http://stackoverflow.com/a/23730146/1644202"/>
        private void SingleInstanceWatcher()
        {
            // check if it is already open.
            try
            {
                // try to open it - if another instance is running, it will exist , if not it will throw
                _eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                // Notify other instance so it could bring itself to foreground.
                _eventWaitHandle.Set();

                // Terminate this instance.
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (_eventWaitHandle.WaitOne())
                {
                    Current.Dispatcher.Invoke(() =>
                    {
                        // could be set or removed anytime
                        if (Current.MainWindow is not null)
                        {
                            var mainWindow = Current.MainWindow;

                            if (mainWindow.WindowState == WindowState.Minimized || mainWindow.Visibility != Visibility.Visible)
                            {
                                mainWindow.Show();
                                mainWindow.WindowState = WindowState.Normal;
                            }

                            // According to some sources these steps are required to be sure it went to foreground.
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
