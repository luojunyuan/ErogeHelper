// See https://aka.ms/new-console-template for more information
using ErogeHelper;
using ErogeHelper.Share;
using ErogeHelper.Share.Languages;
using SplashScreenGdip;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

#region Arguments Check
if (args.Length == 0)
{
    MessageBox.Show(Strings.App_StartNoParameter, (DateTime.Now - Process.GetCurrentProcess().StartTime).Milliseconds.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
    return;
}
var gamePath = args[0];
if (File.Exists(gamePath) && Path.GetExtension(gamePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
{
    gamePath = ShellLink.Shortcut.ReadFromFile(gamePath).LinkTargetIDList.Path;
}
if (!File.Exists(gamePath))
{
    MessageBox.Show(Strings.App_StartInvalidPath + $" \"{gamePath}\"");
    return;
}
#endregion

var splash = new SplashScreen(96, Assembly.GetExecutingAssembly().GetManifestResourceStream("ErogeHelper.assets.klee.png"));
_ = Task.Run(() => splash.Run());

AppLauncher.RunGame(gamePath, args.Contains("-le"));
var pids = AppLauncher.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
if (pids.Length == 0)
{
    splash.Close();
    MessageBox.Show(Strings.App_StartNoParameter);
    return;
}

var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
new IpcMain(pipeServer);

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
