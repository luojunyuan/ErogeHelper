using System.Windows;
using ErogeHelper.Function;
using ErogeHelper.Function.Startup;
using ErogeHelper.View.MainGame;
using Splat;

namespace ErogeHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        if (Utils.IsOrAfter1903)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Function/Properties/ModernWpfDependency.xaml", UriKind.Absolute)
            });
        }
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        MainWindow = new MainGameWindow();

        if (State.MainProcess.HasExited)
        {
            LogHost.Default.Info("MainProcess exited before application initialized");
            Shutdown();
            return;
        }
        else
        {
            State.MainProcess.Exited += (_, _) =>
            {
                LogHost.Default.Info("Detected game quit event");
                Dispatcher.Invoke(() =>
                {
                    MainWindow.Hide();
                    Shutdown();
                });
            };
        }
    }
}
