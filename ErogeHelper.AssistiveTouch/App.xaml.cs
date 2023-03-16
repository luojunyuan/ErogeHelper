using System.Diagnostics;
using System.IO.Pipes;
using System.Windows;
using ErogeHelper.AssistiveTouch.Helper;

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
            }
        }
    }
}
