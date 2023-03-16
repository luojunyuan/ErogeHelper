using System.IO;
using System.Reactive.Linq;
using System.Windows;
using ErogeHelper;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function;
using ErogeHelper.Function.Startup;
using ErogeHelper.Model.Services;

Setup.GlobalExceptionHandling();
if (Setup.SingleInstanceWatcher(args.Contains("-new")))
    return 0;
Strings.Culture = Thread.CurrentThread.CurrentCulture;
if (args.Length == 0)
{
    MessageBox.Show(Strings.App_StartNoParameter, "ErogeHelper", MessageBoxButton.OK, MessageBoxImage.Information);
    return 0;
}
var gamePath = args[0];
if (File.Exists(gamePath) && Path.GetExtension(gamePath).Equals(".lnk"))
{
    gamePath = ShellLink.Shortcut.ReadFromFile(gamePath).LinkTargetIDList.Path;
}
if (!File.Exists(gamePath))
{
    MessageBox.Show(Strings.App_StartInvalidPath + $" \"{gamePath}\"", "ErogeHelper");
    return 0;
}
if (gamePath.EndsWith("ErogeHelper.exe"))
{
    MessageBox.Show(Strings.App_StartItself, "ErogeHelper");
    return 0;
}

Environment.CurrentDirectory = AppContext.BaseDirectory;
var splash = new SplashScreenGdip(96, 96);
Task.Run(() => splash.Run());

// Setup infrastructures
State.Initialze(gamePath);
DI.RegisterServices();
var gameWindowHooker = DependencyResolver.GetService<IGameWindowHooker>();
var fullscreenSource = AppLauncher.FullScreenWatcher(gameWindowHooker);
State.InitFullscreenChanged(fullscreenSource);
AppLauncher.RunGame(State.GamePath, args.Contains("-le"));
var procInfo = AppLauncher.ProcessCollect(Path.GetFileNameWithoutExtension(State.GamePath));
if (procInfo == null)
{
    splash.Close();
    MessageBox.Show(Strings.MessageBox_TimeoutInfo, "ErogeHelper");
    return 0;
}
State.InitGameProcesses(procInfo.Value);
var status = gameWindowHooker.SetupGameWindowHook(State.MainProcess);
if (status == -1)
{
    splash.Close();
    var result = MessageBox.Show("目标程序运行在管理员模式下，建议将目标程序重新以用户权限打开。或者你可以点击“是(Y)”以提升ErogeHelper的权限，“否(N)” ErogeHelper将正常退出", "ErogeHelper", MessageBoxButton.YesNo);
    if (result == MessageBoxResult.Yes)
        Utils.RunWithElevatedEH(gamePath);
    return 0;
}

// Main
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
var app = new App();
Setup.ExceptionHandling(app);
app.MainWindow.ContentRendered += (_, _) => AppLauncher.StartOptionalFunctions();

splash.Close();
return app.Run(app.MainWindow);
