using ErogeHelper.Common;
using ErogeHelper.Common.Function;
using ErogeHelper.Language;
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
            try
            {
                SetupExceptionHandling();
                SetLanguageDictionary();
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
                    var startupService = DependencyInject.GetService<IStartupService>();

                    if (args.Length != 0)
                    {
                        if (args.Contains("-ToastActivated"))
                        {
                            this.Log().Debug("win10 toast activated");
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
                AppExit();
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
                Content = Strings.ErrorDialog_Content,
                ExpandedInformation = $"Caused by {ex.Source}\n" +
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
                this.Log().Debug("Fine exception WaitHandleCannotBeOpenedException for active singleton app");
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // TODO: CustomNotification with enforce get over process
            // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/CustomNotificatios.md
            var notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider =
                    new PrimaryScreenPositionProvider(
                        corner: Corner.BottomRight,
                        offsetX: 16,
                        offsetY: 12);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(5),
                    maximumNotificationCount: MaximumNotificationCount.UnlimitedNotifications());

                cfg.DisplayOptions.TopMost = true;
            });

            Observable.Create<object>(observer =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        observer.OnNext(new object());
                    }
                    return Disposable.Empty;
                })
                .SubscribeOn(ReactiveUI.RxApp.TaskpoolScheduler)
                //.ObserveOnDispatcher()
                .Subscribe(_ =>
                    // Note: Would throw Exceptions if enforce getting over process
                    // System.Threading.Tasks.TaskCanceledException(In System.Private.CoreLib.dll)
                    // System.TimeoutException(In WindowsBase.dll)
                    notifier.ShowInformation(
                        "ErogeHelper is already running!",
                        new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false })
                );
        }

        private static void SetLanguageDictionary() =>
            Strings.Culture = Thread.CurrentThread.CurrentCulture.ToString() switch
            {
                "zh-Hans" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-CN" => new System.Globalization.CultureInfo("zh-Hans"),
                "zh-SG" => new System.Globalization.CultureInfo("zh-Hans"),
                _ => new System.Globalization.CultureInfo(""),
            };
    }
}
