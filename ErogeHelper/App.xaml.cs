using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ErogeHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            // Enable Pointer for touch device
            AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

            // Set thread error handle
            AppDomain.CurrentDomain.UnhandledException += (s, unhandledExceptionArgs) =>
            {
                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher is null && Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                Exception ex = (Exception)unhandledExceptionArgs.ExceptionObject;
                System.Diagnostics.Trace.WriteLine(ex);
            };
            DispatcherUnhandledException += (s, dispatcherUnhandledExceptionEventArgs) =>
            {
                // More friendly
                dispatcherUnhandledExceptionEventArgs.Handled = true;

                System.Diagnostics.Trace.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception);
            };
        }
    }
}
