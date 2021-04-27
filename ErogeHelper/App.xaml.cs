using ErogeHelper.Common.Function;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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
			    Current.Events().Exit.Subscribe(args => AppExit(args.ApplicationExitCode));
                var currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
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

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                if (e.Args.Length != 0)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var view = DependencyInject.GetRequiredService<IViewFor<MainGameViewModel>>() as Window;
                    view.Show();
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
                MainInstruction = ex.Message,
                Content = @$"Eroge Helper run into some trouble. {(string.IsNullOrWhiteSpace(ex.StackTrace) ? string.Empty : "See detail error by click Detail information below.")}",
                ExpandedInformation = ex.StackTrace,
                Width = 300,
            };

            // TODO: Add copy to clipboard button
            TaskDialogButton okButton = new(ButtonType.Ok);
            dialog.Buttons.Add(okButton);
            _ = dialog.ShowDialog();
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

            new Task(() =>
            {
                var isMessageBoxShowed = false;
                while (_eventWaitHandle.WaitOne())
                {
                    if (isMessageBoxShowed) 
                        continue;
                    Current.Dispatcher.InvokeAsync(async () =>
                    {
                        isMessageBoxShowed = true;
                        await ModernWpf.MessageBox.ShowAsync("ErogeHelper is running!", "Eroge Helper").ConfigureAwait(false);
                        isMessageBoxShowed = false;
                    });
                }
            }).Start();
        }
    }
}
