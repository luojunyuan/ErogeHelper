using ModernWpf.Controls;
using System;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace ErogeHelper.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string ShellMenuDllName = "ErogeHelper.ShellMenuHandler.dll";

        public MainWindow()
        {
            InitializeComponent();

            InstallButton.IsEnabled = !ShellExtensionManager.IsInstalled();
            UninstallButton.IsEnabled = ShellExtensionManager.IsInstalled();

            Loaded += CheckNecessaryFile;
        }

        private void CheckNecessaryFile(object sender, RoutedEventArgs e)
        {
            var shellMenuDllPath = Path.Combine(Environment.CurrentDirectory, "ErogeHelper.ShellMenuHandler.dll");
            var serverRegistrationManagerPath = Path.Combine(Environment.CurrentDirectory, "ServerRegistrationManager.exe");
            if (!File.Exists(shellMenuDllPath))
            {
                ModernWpf.MessageBox.Show($"Not found file {shellMenuDllPath}", "Eroge Helper");
                Close();
            }
            else if (!File.Exists(serverRegistrationManagerPath))
            {
                ModernWpf.MessageBox.Show($"Not found file {serverRegistrationManagerPath}", "Eroge Helper");
                Close();
            }
            else if (!IsAdministrator())
            {
                ModernWpf.MessageBox.Show($"Please run as Administrator", "Eroge Helper");
                Close();
            }
        }


        private void Register(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "ServerRegistrationManager.exe",
                Arguments = $"install {ShellMenuDllName} -codebase"
            });

            InstallButton.IsEnabled = false;
            UninstallButton.IsEnabled = true;
        }

        private async void Unload(object sender, RoutedEventArgs e)
        {
            UninstallButton.IsEnabled = false;
            if (DeleteCacheCheckBox.IsChecked ?? false)
            {
                var deleteTipDialog = new ContentDialog
                {
                    Title = ErogeHelper.Language.Strings.Common_Warn,
                    Content = ErogeHelper.Language.Strings.Installer_DeleteTip,
                    PrimaryButtonText = ErogeHelper.Language.Strings.Common_OK,
                    CloseButtonText = ErogeHelper.Language.Strings.Common_Cancel,
                    DefaultButton = ContentDialogButton.Close
                };
                var result = await deleteTipDialog.ShowAsync();
                if (result == ContentDialogResult.None)
                {
                    UninstallButton.IsEnabled = true;
                    return;
                }
                if (result == ContentDialogResult.Primary)
                {
                    var roamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var cacheDir = Path.Combine(roamingDir, "ErogeHelper");
                    if (Directory.Exists(cacheDir))
                    {
                        Directory.Delete(cacheDir, true);
                    }
                }
            }

            // unload ShellHandle.dll first
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "ServerRegistrationManager.exe",
                Arguments = $"uninstall {ShellMenuDllName} -codebase"
            });

            // restart all explore.exe
            var directories = ExplorerHelper.GetOpenedDirectories();
            ExplorerHelper.KillExplorer();
            ExplorerHelper.OpenDirectories(directories);

            InstallButton.IsEnabled = true;
            UninstallButton.IsEnabled = false;
            DeleteCacheCheckBox.IsChecked = false;
        }

        private static bool IsAdministrator()
        {
            var current = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
