using System;
using System.IO;
using System.Windows;
using ModernWpf.Controls;
using Path = System.IO.Path;

namespace ErogeHelper.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        }

        private readonly string shellMenuDllName = "ErogeHelper.ShellMenuHandler.dll";

        private void Register(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "ServerRegistrationManager.exe",
                Arguments = $"install {shellMenuDllName} -codebase"
            });

            InstallButton.IsEnabled = false;
            UninstallButton.IsEnabled = true;
        }

        private async void Unload(object sender, RoutedEventArgs e)
        {
            ContentDialogResult cacheDeleteDialogResult = ContentDialogResult.None;
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
                cacheDeleteDialogResult = await deleteTipDialog.ShowAsync();
                if (cacheDeleteDialogResult == ContentDialogResult.None)
                    return;
            }

            // unload dll first
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "ServerRegistrationManager.exe",
                Arguments = $"uninstall {shellMenuDllName} -codebase"
            });
            // restart all explore.exe
            var helper = new ExplorerHelper();
            helper.CollectDir();
            helper.KillExplorer();
            if (cacheDeleteDialogResult == ContentDialogResult.Primary)
            {
                string roamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var cacheDir = Path.Combine(roamingDir, "ErogeHelper");
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                }
            }
            helper.ReOpenDirs();

            InstallButton.IsEnabled = true;
            UninstallButton.IsEnabled = false;
            DeleteCacheCheckBox.IsChecked = false;
        }
    }
}
