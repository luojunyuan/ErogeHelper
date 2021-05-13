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
//using Windows.UI.Notifications;

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
                //using var scope = _serviceProvider.CreateScope();
                //var serviceProvider = scope.ServiceProvider;
                //var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                //// https://github.com/fluentmigrator/fluentmigrator/issues/1450
                //this.Log().Debug("Fine FileNotFoundExceptions in CLR");
                //runner.MigrateUp(); 

                if (e.Args.Length != 0)
                {
                    if (e.Args.Contains("-ToastActivated"))
                        AppExit(-1);

                    await DependencyInject.GetService<IStartupService>().StartFromCommandLine(e).ConfigureAwait(false);
                }
                else
                {
                    throw new NotImplementedException();
                    //DependencyInject.ShowView<SelectGameViewModel>();
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

        private static void ShowErrorDialog(string errorLevel, Exception ex)
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
                Clipboard.SetText(dialog.WindowTitle + '\n' + ex.Message + '\n' + ex.StackTrace);
            }
        }

        private void AppExit(int exitCode = 0)
        {
            Shutdown(exitCode);
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

            Observable.Create<object>(observer =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        observer.OnNext(new object());
                    }
                    return Disposable.Empty;
                })
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    // Fine System.Runtime.InteropServices.COMException when toast first time
                    //new ToastContentBuilder()
                    //    .AddText("ErogeHelper is running!")
                    //    .Show(toast =>
                    //    { 
                    //        toast.Group = "eh";
                    //        toast.Tag = "eh";
                    //        // TODO: Add Kill ErogeHelper immediately
                    //        //try
                    //        //{ 
                    //        //    // Issue TODO: report bug
                    //        //    //toast.ExpirationTime = DateTimeOffset.Now.AddSeconds(5);
                    //        //}
                    //        //catch(InvalidCastException ex)
                    //        //{ 
                    //        //    this.Log().Debug(ex);    
                    //        //}
                    //    });
                });
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
