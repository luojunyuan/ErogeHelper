// See https://aka.ms/new-console-template for more information
using ErogeHelper;
using SplashScreenGdip;
using System.Diagnostics;
using System.IO.Pipes;

#region Arguments Check
if (args.Length == 0)
{
    MessageBox.Show($"{Strings.App_StartNoParameter}({(DateTime.Now - Process.GetCurrentProcess().StartTime).Milliseconds})");
    return;
}
var gamePath = args[0];
if (File.Exists(gamePath) && Path.GetExtension(gamePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
{
    gamePath = WindowsShortcutFactory.WindowsShortcut.Load(gamePath).Path ?? "Resolve lnk file failed";
}
if (!File.Exists(gamePath))
{
    MessageBox.Show(Strings.App_StartInvalidPath + $" \"{gamePath}\"");
    return;
}
#endregion

var stream = typeof(Program).Assembly.GetManifestResourceStream("ErogeHelper.assets.klee.png") ?? throw new ArgumentException("stream");
var splash = new SplashScreen(96, stream);
_ = Task.Run(() => splash.Run());

#region Start Game
Process? leProc;
try
{
    leProc = AppLauncher.RunGame(gamePath, args.Contains("-le"));// || or contain le.config file
}
catch(InvalidOperationException)
{
    splash.Close();
    MessageBox.Show(Strings.App_LENotInstall);
    return;
}
catch(ArgumentException ex)
{
    splash.Close();
    MessageBox.Show(Strings.App_LENotFound + ex.Message);
    return;
}
var (game, pids) = AppLauncher.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
if (game is null)
{
    splash.Close();
    MessageBox.Show(Strings.App_Timeout);
    return;
}
leProc?.Kill();
#endregion

var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
_ = new IpcMain(pipeServer);

IpcMain.Once(IpcTypes.Loaded, splash.Close);

Environment.CurrentDirectory = AppContext.BaseDirectory;
while (!game.HasExited)
{
    var touch = new Process()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "ErogeHelper.AssistiveTouch.exe",
            Arguments = pipeServer.GetClientHandleAsString() + ' ' + pids[0],
            UseShellExecute = false,
        }
    };
    touch.Start();
    touch.WaitForExit();
    if(touch.ExitCode == -2)
    {
        User32.ShowWindow(splash.WindowHandle, 0);
        MessageBox.Show(Strings.App_Timeout, parent: splash.WindowHandle);
        splash.Close();
        break;
    }
}
