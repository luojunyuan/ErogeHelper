using System;
using System.Threading;
using System.Windows.Threading;
using Serilog;

namespace ErogeHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private App()
        {
            // UNDONE: Check singleton app
            // use toast tip user after 20s, turn ErogeHelper down in TaskManager or EH handle this

            // Enable Pointer for touch device
            AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

            // Set logger
            Serilog.Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                // VS Output
                .WriteTo.Debug(outputTemplate: 
                    "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
#else
				.MinimumLevel.Information()
#endif
                .CreateLogger();

            // Set i18n
            SetLanguageDictionary();

            // Set thread error handle
            AppDomain.CurrentDomain.UnhandledException += (_, unhandledExceptionArgs) =>
            {
                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher is null && Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                var ex = unhandledExceptionArgs.ExceptionObject as Exception ?? new Exception("???");

                Log.Fatal(ex);
            };
            DispatcherUnhandledException += (_, dispatcherUnhandledExceptionEventArgs) =>
            {
                // More friendly
                //dispatcherUnhandledExceptionEventArgs.Handled = true;

                var ex = dispatcherUnhandledExceptionEventArgs.Exception;

                Log.Error(ex);
            };
        }

        private static void SetLanguageDictionary()
        {
            Language.Strings.Culture = Thread.CurrentThread.CurrentCulture.ToString() switch
            {
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                // Default english because there can be so many different system language, we rather fallback on
                // english in this case.
                _ => new System.Globalization.CultureInfo(string.Empty),
            };
        }
    }
}
