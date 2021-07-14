using CommunityToolkit.WinUI.Notifications;
using ErogeHelper.Common;
using ErogeHelper.Common.Contract;
using ErogeHelper.Common.Function;
using ErogeHelper.Model.Service.Interface;
using Ookii.Dialogs.Wpf;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace ErogeHelper
{
    public partial class App : IEnableLogger
    {
        public App()
        {
            // Note: This application would throw some exceptions that wasn't bugs when initializing
            // System.Threading.WaitHandleCannotBeOpenedException for active singleton app
            // https://github.com/reactiveui/ReactiveUI/issues/2395
            // System.IO.FileNotFoundException 5 times cause reactiveUI is scanning Drawing, XamForms, Winforms, etc
            // System.Runtime.InteropServices.COMException when access ToastComponent first time
            try
            {
                SetupExceptionHandling();
                SetI18NLanguageDictionary();
                SingleInstanceWatcher();

                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
                Directory.SetCurrentDirectory(currentDirectory ??
                    throw new ArgumentNullException(nameof(currentDirectory)));

                DependencyInject.Register();

                this.Events().Startup
                    .Select(startupEvent => startupEvent.Args)
                    .Subscribe(async args =>
                {
                    ToastNotificationManagerCompat.OnActivated += toastArgs =>
                    {
                        if (toastArgs.Argument.Length == 0)
                        {
                            this.Log().Debug("Toast Clicked");
                            return;
                        }
                        var toastArguments = ToastArguments.Parse(toastArgs.Argument);
                        this.Log().Debug(toastArguments);
                    };

                    var startupService = DependencyInject.GetService<IStartupService>();

                    if (args.Length != 0)
                    {
                        // EH already exit, but toast is clicked. This one shouldn't happen 
                        if (args.Contains("-ToastActivated") || args.Contains("-Embedding"))
                        {
                            AppExit(-1);
                        }

                        if (args.Contains("-Debug"))
                            throw new NotImplementedException();

                        await startupService.StartFromCommandLine(args.ToList()).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                });
            }
            catch (AppAlreadyExistsException)
            {
                Current.Shutdown();
            }
            catch (Exception ex)
            {
                this.Log().Error(ex);
                ShowErrorDialog("AppCtor", ex);
                AppExit(-1);
            }
        }

        public static void AppExit(int exitCode = 0)
        {
            Current.Dispatcher.Invoke(() => Current.Shutdown(exitCode));
            if (Utils.IsOSWindows8OorNewer)
            {
                ToastNotificationManagerCompat.History.Clear();
            }

            if (exitCode != 0)
            {
                Environment.Exit(exitCode);
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (Dispatcher.FromThread(Thread.CurrentThread) is null ||
                    Dispatcher.CurrentDispatcher.Thread == Thread.CurrentThread)
                    return;

                var ex = args.ExceptionObject as Exception ??
                         new Exception("AppDomain.CurrentDomain.UnhandledException");

                this.Log().Fatal(ex);
                ShowErrorDialog("Fatal", ex);
                AppExit(-1);
            };

            DispatcherUnhandledException += (_, args) =>
            {
                var ex = args.Exception;

                this.Log().Error(ex);

                if (Windows.Count != 0)
                {
                    args.Handled = true;
                    ShowErrorDialog("UI", ex);
                }
                else
                {
                    ShowErrorDialog("UI-Fatal", ex);
                    AppExit(-1);
                }
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                args.SetObserved();
                this.Log().Error(args.Exception);
                ShowErrorDialog("Task", args.Exception);
            };
        }

        private static void ShowErrorDialog(string errorLevel, Exception ex)
        {
            using var dialog = new TaskDialog
            {
                WindowTitle = $"ErogeHelper v{Utils.AppVersion} - {errorLevel} Error",
                MainInstruction = $"{ex.GetType().FullName}: {ex.Message}",
                Content = Language.Strings.ErrorDialog_Content,
                ExpandedInformation = $"OS Version: {Utils.GetOSInfo()}\n" +
                                      $"Caused by source `{ex.Source}`\n" +
                                      ex.StackTrace +
                                      (ex.InnerException is null ?
                                          string.Empty :
                                          $"\nThis Exception caused with in another Exception: {ex.InnerException}"),
                Width = 300,
            };

            TaskDialogButton clipboardButton = new(Language.Strings.OokiiDialog_ClipboardLabel);
            TaskDialogButton okButton = new(ButtonType.Ok);
            dialog.Buttons.Add(clipboardButton);
            dialog.Buttons.Add(okButton);
            var clicked = dialog.ShowDialog();
            if (clicked == clipboardButton)
            {
                var errorInfo = dialog.WindowTitle + '\n' + dialog.MainInstruction + '\n' + dialog.ExpandedInformation;
                Current.Dispatcher.Invoke(() => Clipboard.SetText(errorInfo));
            }
        }

        private const string UniqueEventName = "{d3bcc592-a3bb-4c26-99c4-23c750ddcf77}";
        private EventWaitHandle? _eventWaitHandle;

        private void SingleInstanceWatcher()
        {
            try
            {
                _eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                _eventWaitHandle.Set();

                throw new AppAlreadyExistsException();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            var toastLifetimeTimer = new System.Diagnostics.Stopwatch();

            Observable.Create<object>(observer =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        observer.OnNext(new object());
                    }
                    return Disposable.Empty;
                })
                .SubscribeOn(ReactiveUI.RxApp.TaskpoolScheduler)
                .Subscribe(async _ =>
                {
                    if (Utils.IsOSWindows8OorNewer)
                    { 
                        new ToastContentBuilder()
                            .AddText("ErogeHelper is running!")
                            .Show(toast =>
                            {
                                toast.Group = "eh";
                                toast.Tag = "eh";
                            // ExpirationTime bugged with InvalidCastException in .Net5
                            //toast.ExpirationTime = DateTime.Now.AddSeconds(5);
                        });

                        toastLifetimeTimer.Restart();
                        await Task.Delay(ConstantValues.ToastDuration).ConfigureAwait(false);
                        if (toastLifetimeTimer.ElapsedMilliseconds >= ConstantValues.ToastDuration)
                        {
                            ToastNotificationManagerCompat.History.Clear();
                            toastLifetimeTimer.Stop();
                        }
                    }
                    else
                    {
                        Utils.DesktopNotifier.ShowInformation(
                            "ErogeHelper is running!",
                            new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false });
                    }
                });
        }

        private static void SetI18NLanguageDictionary() =>
            Language.Strings.Culture = Thread.CurrentThread.CurrentCulture.ToString() switch
            {
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-SG" => new System.Globalization.CultureInfo("zh-Hans"),
                _ => new System.Globalization.CultureInfo(""),
            };
    }
}
