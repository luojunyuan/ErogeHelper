using ErogeHelper.AssistiveTouch.Helper;
using ErogeHelper.AssistiveTouch.NativeMethods;
using System.Diagnostics;
using System.IO.Pipes;
using System.Windows;
using WindowsInput.Events;

namespace ErogeHelper.AssistiveTouch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Process GameProcess { get; private set; } = null!;

        public static IntPtr GameWindowHandle { get; private set; } = IntPtr.Zero;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                var _pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, e.Args[0]);
                new IpcRenderer(_pipeClient);

                GameProcess = Process.GetProcessById(int.Parse(e.Args[1]));

                if (GameProcess is null)
                {
                    Environment.Exit(-1);
                    return;
                }

                GameWindowHandle = HwndTools.FindMainWindowHandle(GameProcess);
                //GameWindowHandle = GameProcess.MainWindowHandle;

                I18n.LoadLanguage();

                Config.Load();

                if (Config.EnterKeyMapping)
                {
                    GlobalKeyHook();
                }
            }
        }

        private static void GlobalKeyHook()
        {
            const int debounce = 40;
            var throttle = new Throttle<int>(debounce, _ =>
                WindowsInput.Simulate.Events()
                    .Click(KeyCode.Enter)
                    .Invoke());
            // Thread name MessagePumpingObject
            var keyboard = WindowsInput.Capture.Global.KeyboardAsync();
            keyboard.KeyDown += (_, e) =>
            {
                if (e.Data.Key == KeyCode.Z && User32.GetForegroundWindow() == GameWindowHandle)
                {
                    e.Next_Hook_Enabled = false;
                    throttle.Signal(default);
                }
            };
            Current.Exit += (_, _) => keyboard.Dispose();
        }
    }
}
