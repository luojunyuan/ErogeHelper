using System.Windows;
using ErogeHelper.Common.Languages;
using Ookii.Dialogs.Wpf;

namespace ErogeHelper.Function.Startup;

/// <summary>
/// Top-level enviroment initialze helper.
/// </summary>
internal static class Setup
{
    private const string UniqueAppEventName = "{d3bcc592-a3bb-4c26-99c4-23c750ddcf77}";

    /// <returns>True if there is another eh instance.</returns>
    public static bool SingleInstanceWatcher(bool allowedNew)
    {
        var eventWaitHandle =
            new EventWaitHandle(false, EventResetMode.AutoReset, UniqueAppEventName, out var createdNew);

        if (!createdNew && !allowedNew)
        {
            eventWaitHandle.Set();
            return true;
        }

        new TaskFactory().StartNew(() =>
        {
            Thread.CurrentThread.Name = "Sington App Listener";
            while (eventWaitHandle.WaitOne())
            {
                //if DI registerted then toast
            }
        }, TaskCreationOptions.LongRunning);

        return false;
    }

    // TODO: Split to two
    public static void GlobalExceptionHandling()
    {
#if RELEASE
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (Application.Current is not null &&
                Application.Current.Dispatcher.Thread.ManagedThreadId == Environment.CurrentManagedThreadId)
            {
                return;
            }

            var ex = args.ExceptionObject as Exception ?? new("blank");

            ShowErrorDialog("Fatal", ex);
            Environment.Exit(-1);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            args.SetObserved();
            ShowErrorDialog("Task", args.Exception);
        };
#endif
    }
    public static void ExceptionHandling(Application app)
    {
        // Active after App.Run()
#if RELEASE
        app.DispatcherUnhandledException += (_, args) =>
        {
            var ex = args.Exception;

            if (app is not null &&
                app.MainWindow.HasContent)
            {
                args.Handled = true;
                ShowErrorDialog("UI", ex);
                return;
            }

            var additionInfo = string.Empty;
            if (ex.HResult == -2003303418) // 0x88980406 WPF render thread failures
                additionInfo = "Please try disable windows transparency effects";

            ShowErrorDialog("UI-Fatal", ex, additionInfo);
            Environment.Exit(-1);
        };
#endif
    }

    private static void ShowErrorDialog(string errorLevel, Exception ex, string additionInfo = "")
    {
        var extentInfo = string.Format(Strings.App_ErrorDialogExInfo, Utils.GetOsInfo(), ex.Source) +
            ex.StackTrace + (ex.InnerException is null ? string.Empty :
                $"\r\nThis Exception caused with in another Exception: {ex.InnerException}");

        using var dialog = new TaskDialog
        {
            WindowTitle = string.Format(Strings.App_ErrorDialog, EHContext.EHVersion, errorLevel),
            MainInstruction = $@"{ex.GetType().FullName}: {ex.Message}",
            Content = Strings.ErrorDialog_Content + ' ' + additionInfo,
            ExpandedInformation = extentInfo, // Error stack
            Width = 300
        };

        var clipboardButton = new TaskDialogButton(Strings.OokiiDialog_ClipboardLabel);
        var okButton = new TaskDialogButton(ButtonType.Ok);
        dialog.Buttons.Add(clipboardButton);
        dialog.Buttons.Add(okButton);
        var clicked = dialog.ShowDialog();
        if (clicked != clipboardButton)
            return;

        var errorInfo = dialog.WindowTitle + "\r\n" + dialog.MainInstruction + "\r\n" + dialog.ExpandedInformation;
        Clipboard.SetText(errorInfo);
    }
}
