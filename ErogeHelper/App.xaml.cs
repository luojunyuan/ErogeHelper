using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using ErogeHelper.Language;
using Ookii.Dialogs.Wpf;

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
            // see Windows.UI.Notifications; or ToastNotifications
            // Toast user ErogeHelper is running, or you can turn ErogeHelper down immediately

            // Enable Pointer for touch device
            //AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

            // Set environment to app directory
            var currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            Directory.SetCurrentDirectory(currentDirectory ?? throw new ArgumentNullException(nameof(currentDirectory),
                                                            @"Could not located Eroge Helper's directory"));

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

            // Set thread error handle
            AppDomain.CurrentDomain.UnhandledException += (_, unhandledExceptionArgs) =>
            {
                if (Dispatcher.FromThread(Thread.CurrentThread) is null || 
                    Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                var ex = unhandledExceptionArgs.ExceptionObject as Exception ?? new Exception("???");

                // UNDONE: Clear sub process
                ShowErrorDialog("Fatal", ex);

                Log.Fatal(ex);
            };
            DispatcherUnhandledException += (_, dispatcherUnhandledExceptionEventArgs) =>
            {
                // More friendly
                dispatcherUnhandledExceptionEventArgs.Handled = true;

                var ex = dispatcherUnhandledExceptionEventArgs.Exception;

                ShowErrorDialog("UI", ex);
                Log.Error(ex);
            };
        }

        private static void ShowErrorDialog(string errorLevel, Exception ex)
        {
            using var dialog = new TaskDialog
            {
                WindowTitle = $"Eroge Helper - {errorLevel} Error",
                MainInstruction = ex.Message,
                Content =
                    "Eroge Helper run into some trouble. See detail error by click Detail information below.",
                ExpandedInformation = ex.InnerException?.ToString(),
            };

            TaskDialogButton okButton = new(ButtonType.Ok);
            dialog.Buttons.Add(okButton);
            _ = dialog.ShowDialog();
        }
    }
}
