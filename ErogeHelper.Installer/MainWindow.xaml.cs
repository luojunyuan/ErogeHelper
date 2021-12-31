using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using ErogeHelper.Shared;
using ModernWpf.Controls;

namespace ErogeHelper.Installer
{
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

        private async void UnInstall(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;
            UninstallButton.IsEnabled = false;

            if (DeleteCacheCheckBox.IsChecked ?? false)
            {
                var deleteTipDialog = new ContentDialog
                {
                    Title = Shared.Languages.Strings.Common_Warning,
                    Content = Shared.Languages.Strings.Installer_DeleteTip,
                    PrimaryButtonText = Shared.Languages.Strings.Common_OK,
                    CloseButtonText = Shared.Languages.Strings.Common_Cancel,
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

                    // TODO: Arm64 installer unload dll progress
                    if (Utils.IsArm)
                    {
                        Process.GetProcessesByName("dllhost").ToList()
                            .ForEach(p =>
                            {
                                try
                                {
                                    var accessMainModule = p.MainModule!.BaseAddress;
                                    p.Kill();
                                }
                                catch { }
                            });
                    }

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

        // https://stackoverflow.com/questions/21566528/what-happens-if-i-remove-the-auto-added-supportedruntime-element
        /// <summary>
        /// Throws an exception if the current version of the .NET Framework is smaller than the specified
        /// <see cref="supportedVersion"/>.
        /// </summary>
        /// <param name="supportedVersion">The minimum supported version of the .NET Framework on which the current
        /// program can run.
        /// The version to use is not the marketing version, but the file version of mscorlib.dll.
        /// See <see href="https://blogs.msdn.microsoft.com/dougste/2016/03/17/file-version-history-for-clr-4-x/">File 
        /// version history for CLR 4.x</see> and/or <see href="https://it.wikipedia.org/wiki/.NET_Framework#Versioni">
        /// .NET Framework Versioni (Build pubblicata)</see> for the version to use.
        /// </param>
        /// <exception cref="NotSupportedException">The current version of the .NET Framework is smaller than the 
        /// specified <see cref="supportedVersion"/>.</exception>
        /// <returns>The version of the .NET Framework on which the current program is running.</returns>
        public static Version EnsureSupportedDotNetFrameworkVersion(Version supportedVersion)
        {
            var fileVersion = typeof(int).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            var currentVersion = new Version(fileVersion!.Version);
            MessageBox.Show(currentVersion.ToString());
            if (currentVersion < supportedVersion)
                throw new NotSupportedException(
                    $"Microsoft .NET Framework {supportedVersion} or newer is required. " +
                    $"Current version ({currentVersion}) is not supported.");
            return currentVersion;
        }
    }
}
