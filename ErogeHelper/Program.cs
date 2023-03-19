// See https://aka.ms/new-console-template for more information
using ErogeHelper;
using ErogeHelper.IpcChannel;
using SplashScreenGdip;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

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

var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ErogeHelper.assets.klee.png") ?? throw new ArgumentException("stream");
var splash = new SplashScreen(96, stream);
_ = Task.Run(() => splash.Run());

AppLauncher.RunGame(gamePath, args.Contains("-le"));// || or contain le.config file
var pids = AppLauncher.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
if (pids.Length == 0)
{
    splash.Close();
    MessageBox.Show(Strings.App_StartNoParameter);
    return;
}

var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
_ = new IpcMain(pipeServer);

Environment.CurrentDirectory = AppContext.BaseDirectory;
var touch = new Process()
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "ErogeHelper.AssistiveTouch.exe", 
        Arguments = pipeServer.GetClientHandleAsString() + ' ' + string.Join(" ", pids),
        UseShellExecute = false,
    }
};

IpcMain.Once(IpcTypes.Loaded, splash.Close);

touch.Start();

touch.WaitForExit(); // Game.WaitForExit();
