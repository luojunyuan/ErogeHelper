using Serilog;
using System;
using System.IO;
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
            // Set environment to app directory
            var currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location)!;
            Directory.SetCurrentDirectory(currentDirectory);

            // Switch on Caliburn.Micro.ViewModelBinder debug monitor
            //var baseGetLog = LogManager.GetLog;
            //LogManager.GetLog = t => t == typeof(ViewModelBinder) ? new DebugLog(t) : baseGetLog(t);

            // Set logger
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug() // VS Output
                .CreateLogger();

            // Set i18n
            SetLanguageDictionary();

            // Set thread error handle
            AppDomain.CurrentDomain.UnhandledException += (s, unhandledExceptionArgs) =>
            {
                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher is null && Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                Exception ex = (Exception)unhandledExceptionArgs.ExceptionObject;
                Log.Error(ex);
                ModernWpf.MessageBox.Show(ex.Message, "Eroge Helper - Unhandled Fatal Error");
            };
            DispatcherUnhandledException += (s, dispatcherUnhandledExceptionEventArgs) =>
            {
                dispatcherUnhandledExceptionEventArgs.Handled = true; // More friendly

                Log.Error(dispatcherUnhandledExceptionEventArgs.Exception);
                ModernWpf.MessageBox.Show(dispatcherUnhandledExceptionEventArgs.Exception.Message, "Eroge Helper - Unhandled UI Error");
            };
        }

        private static void SetLanguageDictionary()
        {
            Language.Strings.Culture = (Thread.CurrentThread.CurrentCulture.ToString()) switch
            {
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                // default english because there can be so many different system language, we rather fallback on 
                // english in this case.
                _ => new System.Globalization.CultureInfo(""),
            };
        }
    }
}
