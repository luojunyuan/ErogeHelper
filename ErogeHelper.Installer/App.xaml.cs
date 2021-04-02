using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // initiate it. Call it first.
            SingleInstanceWatcher();
        }

        private const string UniqueEventName = "{2d0ccd54-f861-46be-9804-43aff3775111}";
        private EventWaitHandle _eventWaitHandle = null!;

        /// <summary>prevent a second instance and signal it to bring its MainWindow to foreground</summary>
        /// <seealso cref="https://stackoverflow.com/a/23730146/1644202"/>
        private void SingleInstanceWatcher()
        {
            // check if it is already open.
            try
            {
                // try to open it - if another instance is running, it will exist , if not it will throw
                this._eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                // Notify other instance so it could bring itself to foreground.
                this._eventWaitHandle.Set();

                // Terminate this instance.
                this.Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                this._eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (this._eventWaitHandle.WaitOne())
                {
                    Current.Dispatcher.InvokeAsync(() =>
                    {
                        // could be set or removed anytime
                        if (!Current.MainWindow.Equals(null))
                        {
                            var mw = Current.MainWindow;

                            if (mw.WindowState == WindowState.Minimized || mw.Visibility != Visibility.Visible)
                            {
                                mw.Show();
                                mw.WindowState = WindowState.Normal;
                            }

                            // According to some sources these steps are required to be sure it went to foreground.
                            mw.Activate();
                            mw.Topmost = true;
                            mw.Topmost = false;
                            mw.Focus();
                        }
                    });
                }
            })
            .Start();
        }
    }
}
