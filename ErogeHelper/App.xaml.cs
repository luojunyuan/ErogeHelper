using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.WinUI.Notifications;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Exceptions;
using ErogeHelper.Shared.Languages;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using Splat;
using MessageBox = ModernWpf.MessageBox;

namespace ErogeHelper;

public partial class App : IEnableLogger
{
    private const string UniqueAppEventName = "{d3bcc592-a3bb-4c26-99c4-23c750ddcf77}";

    public App()
    {
        try
        {
            SetupExceptionHandling();
            SingleInstanceWatcher();

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Startup += (_, startupEvent) =>
            {
                var args = startupEvent.Args;
                // EH already exit, but toast is clicked. This one shouldn't happen 
                if (args.Contains("-ToastActivated") || args.Contains("-Embedding"))
                {
                    Terminate(-1);
                    return;
                }

                if (args.Length == 0)
                {
                    MessageBox.Show(Strings.App_StartNoParameter, "Eroge Helper");
                    Terminate();
                    return;
                }

                var gameFullPath = args[0];
                if (gameFullPath.Equals(Environment.ProcessPath, StringComparison.Ordinal))
                {
                    MessageBox.Show(Strings.App_StartItself, "Eroge Helper");
                    Terminate();
                    return;
                }

                DisablePointerMessage.Apply();
                ToastManagement.Register();
                ToastManagement.IfAdminThenToast();
                DI.UpdateDatabase();
                DI.RegisterServices();
                DI.WarmingUp();

                AppLauncher.StartFromCommandLine(gameFullPath, args.Any(arg => arg is "/le" or "-le"));
            };
        }
        catch (AppExistedException)
        {
            Terminate(1);
        }
        catch (Exception ex)
        {
            this.Log().Error(ex);
            ShowErrorDialog("AppCtor", ex);
            Terminate(-1);
        }
    }

    public static string EHVersion { get; } 
        = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "9.9.9.9";

    /// <param name="exitCode">-1 abnormal, 0 normal, 1 by another instance</param>
    public static void Terminate(int exitCode = 0)
    {
        if (Current is not null)
        {
            Current.Windows
                .Cast<Window>().ToList()
                .ForEach(w => w.Close());

            Current.Shutdown(exitCode);
        }

        if (exitCode != 1 && Utils.HasWinRT)
        {
            try
            {
                ToastNotificationManagerCompat.History.Clear();
            }
            catch (COMException ex)
            {
                // When run on early system like 1507 1511 for the first time would throw error #16
                LogHost.Default.Debug(ex.Message);
            }
        }

        if (exitCode == -1)
        {
            Environment.Exit(exitCode);
        }
    }

    private static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) is null ||
                Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
            {
                return;
            }

            var ex = args.ExceptionObject as Exception ??
                     new ArgumentNullException();

            LogHost.Default.Fatal(ex, "non-UI thread error occurrent");
            ShowErrorDialog("Fatal", ex);
            Terminate(-1);
        };

        Current.DispatcherUnhandledException += (_, args) =>
        {
            var ex = args.Exception;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Current is not null && 
                Current.Windows.Cast<Window>()
                    .Any(window => window.Title.Equals("MainGameWindow", StringComparison.Ordinal)))
            {
                args.Handled = true;
                LogHost.Default.Error(ex, "UI thread error occurrent");
                ShowErrorDialog("UI", ex);
                return;
            }

            var additionInfo = string.Empty;
            if (ex.HResult == -2003303418) // 0x88980406 WPF render thread failures
            {
                additionInfo = "Please try disable windows transparency effects in Settings-Personalization-Color";
            }

            LogHost.Default.Fatal(ex, "UI thread error occurrent");
            ShowErrorDialog("UI-Fatal", ex, additionInfo);
            Terminate(-1);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            args.SetObserved();
            LogHost.Default.Error(args.Exception, "TPL error occurrent");
            ShowErrorDialog("Task", args.Exception);
        };
    }

    private static void SingleInstanceWatcher()
    {
        var eventWaitHandle =
            new EventWaitHandle(false, EventResetMode.AutoReset, UniqueAppEventName, out bool createdNew);

        if (!createdNew)
        {
            eventWaitHandle.Set();
            throw new AppExistedException();
        }

        var toastLifetimeTimer = new Stopwatch();

        Observable.Create<Unit>(observer =>
            {
                while (eventWaitHandle.WaitOne())
                {
                    observer.OnNext(Unit.Default);
                }

                return Disposable.Empty;
            })
            .SubscribeOn(RxApp.TaskpoolScheduler)
            .Subscribe(_ => ToastManagement
                .ShowAsync("ErogeHelper is running!", toastLifetimeTimer)
                .ConfigureAwait(false)
            );
    }

    private static void ShowErrorDialog(string errorLevel, Exception ex, string additionInfo = "")
    {
        using var dialog = new TaskDialog
        {
            WindowTitle = string.Format(
                CultureInfo.CurrentCulture, Strings.App_ErrorDialog, EHVersion, errorLevel),
            MainInstruction = $@"{ex.GetType().FullName}: {ex.Message}",
            Content = Strings.ErrorDialog_Content + (additionInfo == string.Empty ? string.Empty :
                                                                                    "\r\n" + additionInfo),
            ExpandedInformation = string.Format(
                CultureInfo.CurrentCulture, Strings.App_ErrorDialogExInfo, Utils.GetOsInfo(), ex.Source) +
                                  ex.StackTrace +
                                  (ex.InnerException is null ? string.Empty :
                                  $"\r\nThis Exception caused with in another Exception: {ex.InnerException}"),
            Width = 300,
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
