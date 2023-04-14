using System.Diagnostics;
using System.IO.Pipes;
using ErogeHelper;
using SplashScreenGdip;

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

var stream = typeof(Program).Assembly.GetManifestResourceStream("ErogeHelper.assets.klee.png")!;
var splash = new SplashScreen(96, stream);
new Thread(() => PreProcessing(args.Contains("-le"), gamePath, splash)).Start();

splash.Run();

static void PreProcessing(bool leEnable, string gamePath, SplashScreen splash)
{
    #region Start Game
    Process? leProc;
    try
    {
        leProc = AppLauncher.RunGame(gamePath, leEnable);
    }
    catch (ArgumentException ex) when (ex.Message == string.Empty)
    {
        MessageBox.ShowX(Strings.App_LENotSetup, splash);
        return;
    }
    catch (ArgumentException ex) when (ex.Message != string.Empty)
    {
        MessageBox.ShowX(Strings.App_LENotFound + ex.Message, splash);
        return;
    }
    catch (InvalidOperationException)
    {
        MessageBox.ShowX(Strings.App_LENotSupport, splash);
        return;
    }

    var (game, pids) = AppLauncher.ProcessCollect(Path.GetFileNameWithoutExtension(gamePath));
    if (game is null)
    {
        MessageBox.ShowX(Strings.App_Timeout, splash);
        return;
    }
    leProc?.Kill();
    #endregion

    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    _ = new IpcMain(pipeServer);

    IpcMain.Once("Loaded", splash.Close);

    Environment.CurrentDirectory = AppContext.BaseDirectory;
    while (!game.HasExited)
    {
        var gameWindowHandle = AppLauncher.FindMainWindowHandle(game);
        if (gameWindowHandle == IntPtr.Zero) // process exit
        {
            splash.Close();
            break;
        }
        else if (gameWindowHandle.ToInt32() == -1) // FindHandleFailed
        {
            MessageBox.ShowX(Strings.App_Timeout, splash);
            break;
        }

        var touch = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ErogeHelper.AssistiveTouch.exe",
                Arguments = pipeServer.GetClientHandleAsString() + ' ' + gameWindowHandle,
                UseShellExecute = false,
            }
        };
        touch.Start();
        touch.WaitForExit();
    }
}
