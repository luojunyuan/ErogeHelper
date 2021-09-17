using ModernWpf.Controls;
using System;
using System.Diagnostics;
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
                ModernWpf.MessageBox.Show($"File {shellMenuDllPath} does not exist", "Eroge Helper");
                Close();
            }
            else if (!File.Exists(serverRegistrationManagerPath))
            {
                ModernWpf.MessageBox.Show($"File {serverRegistrationManagerPath} does not exist", "Eroge Helper");
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
            InstallButton.IsEnabled = false;
            UninstallButton.IsEnabled = false;

            Process _srm = new()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "ServerRegistrationManager.exe",
                    Arguments = $"install {ShellMenuDllName} -codebase",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            var outputData = string.Empty;
            var errorData = string.Empty;
            _srm.OutputDataReceived += (_, e) => outputData += e.Data + '\n';
            _srm.ErrorDataReceived += (_, e) => errorData += e.Data;
            _srm.Exited += (_, _) =>
            {
                if (!outputData.Contains("error"))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InstallButton.IsEnabled = false;
                        UninstallButton.IsEnabled = true;
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ModernWpf.MessageBox.Show(outputData);
                        InstallButton.IsEnabled = true;
                        UninstallButton.IsEnabled = false;
                    });
                }
            };

            _srm.Start();
            _srm.BeginErrorReadLine();
            _srm.BeginOutputReadLine();
        }

        private async void Unload(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;
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
            Process _srm = new()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "ServerRegistrationManager.exe",
                    Arguments = $"uninstall {ShellMenuDllName} -codebase",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            var outputData = string.Empty;
            var errorData = string.Empty;
            _srm.OutputDataReceived += (_, e) => outputData += e.Data + '\n';
            _srm.ErrorDataReceived += (_, e) => errorData += e.Data;
            _srm.Exited += (_, _) =>
            {
                if (!outputData.Contains("error"))
                {
                    // restart all explore.exe
                    var directories = ExplorerHelper.GetOpenedDirectories();
                    ExplorerHelper.KillExplorer();
                    ExplorerHelper.OpenDirectories(directories);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InstallButton.IsEnabled = true;
                        UninstallButton.IsEnabled = false;
                        DeleteCacheCheckBox.IsChecked = false;
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ModernWpf.MessageBox.Show(outputData);
                        InstallButton.IsEnabled = false;
                        UninstallButton.IsEnabled = true;
                    });
                }
            };

            _srm.Start();
            _srm.BeginErrorReadLine();
            _srm.BeginOutputReadLine();
        }

        private static bool IsAdministrator()
        {
            var current = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
