using ErogeHelper.Common;
using ErogeHelper.Model.Service.Interface;
using Ookii.Dialogs.Wpf;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
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
                SingleInstanceWatcher();
                SetLanguageDictionary();
                Current.Events().Exit.Subscribe(args => AppExit(args.ApplicationExitCode));
                var currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
                Directory.SetCurrentDirectory(currentDirectory ??
                    throw new ArgumentNullException(nameof(currentDirectory)));
                DependencyInject.Register();
            }
            catch (Exception ex)
            {
                this.Log().Error(ex);
                ShowErrorDialog("App", ex);
                AppExit(-1);
            }
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                var startupService = DependencyInject.GetService<IStartupService>();
                //using var scope = _serviceProvider.CreateScope();
                //var serviceProvider = scope.ServiceProvider;
                //var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                //// https://github.com/fluentmigrator/fluentmigrator/issues/1450
                //this.Log().Debug("Fine FileNotFoundExceptions in CLR");
                //runner.MigrateUp(); 

                if (e.Args.Length != 0)
                {
                    if (e.Args.Contains("-ToastActivated"))
                    {
                        this.Log().Debug("win10 toast activated");
                        AppExit(-1);
                    }

                    if (e.Args.Contains("-Debug"))
                        throw new NotImplementedException();
                            
                    await startupService.StartFromCommandLine(e).ConfigureAwait(false);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                this.Log().Error(ex);
                ShowErrorDialog("OnStartup", ex);
                AppExit(-1);
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

            Current.Events().DispatcherUnhandledException.Subscribe(args =>
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
            });

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                args.SetObserved();
                this.Log().Error(args.Exception);
                ShowErrorDialog("Task", args.Exception);
            };
        }

        private static async void ShowErrorDialog(string errorLevel, Exception ex)
        {
            using var dialog = new TaskDialog
            {
                WindowTitle = $@"Eroge Helper - {errorLevel} Error",
                MainInstruction = ex.GetType().FullName + @": " + ex.Message,
                Content = @$"Eroge Helper run into some trouble. {(string.IsNullOrWhiteSpace(ex.StackTrace)
                    ? string.Empty
                    : "See error stack by click Detail information below.")}",
                ExpandedInformation = ex.StackTrace,
                Width = 300,
            };

            TaskDialogButton clipboardButton = new(Language.Strings.OokiiDialog_ClipboardLabel);
            TaskDialogButton okButton = new(ButtonType.Ok);
            dialog.Buttons.Add(clipboardButton);
            dialog.Buttons.Add(okButton);
            var clicked = dialog.ShowDialog();
            if (clicked == clipboardButton)
            {
                await Current.Dispatcher.InvokeAsync(() => 
                    Clipboard.SetText(dialog.WindowTitle + '\n' + ex.Message + '\n' + ex.StackTrace));
            }
        }

        public static void AppExit(int exitCode = 0)
        {
            Current.Dispatcher.InvokeAsync(() => Current.Shutdown(exitCode));
            if (exitCode != 0)
            {
                Environment.Exit(exitCode);
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
                AppExit(-1);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                this.Log().Debug("Fine exception WaitHandleCannotBeOpenedException for active singleton app");
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

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
                        observer.OnNext(new ());
                    }
                    return Disposable.Empty;
                })
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .ObserveOnDispatcher()
                .Subscribe(_ => notifier.ShowInformation(
                    "ErogeHelper is already running!", 
                    new MessageOptions { ShowCloseButton = false, FreezeOnMouseEnter = false })
                );
        }

        private static void SetLanguageDictionary()
        {
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
}
