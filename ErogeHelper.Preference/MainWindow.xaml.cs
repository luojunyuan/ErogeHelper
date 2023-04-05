using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace ErogeHelper.Preference
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigFolder = Path.Combine(RoamingPath, "ErogeHelper");
        private static readonly string ConfigFilePath = Path.Combine(RoamingPath, "ErogeHelper", "EHConfig.ini");

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var config = new IniFile(ConfigFilePath);
            OldScreenShot.IsChecked = bool.Parse(config.Read("ScreenShotTradition") ?? "false");
            ZtwoEnter.IsChecked = bool.Parse(config.Read("EnterKeyMapping") ?? "false");

            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            ButtonUninstall.IsEnabled = ShellExtensionManager.IsInstalled(false);
        }

        private void OldScreenShot_Click(object sender, RoutedEventArgs e)
        {
            var config = new IniFile(ConfigFilePath);
            config.Write("ScreenShotTradition", OldScreenShot.IsChecked.ToString());
        }

        private void ZtwoEnter_Click(object sender, RoutedEventArgs e)
        {
            var config = new IniFile(ConfigFilePath);
            config.Write("EnterKeyMapping", ZtwoEnter.IsChecked.ToString());
        }

        private void ButtonInstallOnClick(object sender, RoutedEventArgs e)
        {
            Installer.DoRegister(false);

            Installer.NotifyShell();

            MessageBox.Show(this, "Register finished. Right click any executable and enjoy :)\r\n" +
                            "\r\n" +
                            "PS: A reboot (or restart of \"explorer.exe\") is required if you are upgrading from an old version.",
                "ErogeHelper",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ButtonUninstall.IsEnabled = true;
        }
        private void ButtonUninstallOnClick(object sender, RoutedEventArgs e)
        {
            Installer.DoUnRegister(false);

            Installer.NotifyShell();

            MessageBox.Show(this, "Unload finished. Thanks for using Eroge Helper :)\r\n" +
                            "\r\n" +
                            "PS: A reboot is required to unlock some components.",
                "ErogeHelper",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ButtonUninstall.IsEnabled = false;
        }

        private void ReExplorerBututonOnClick(object sender, RoutedEventArgs e)
        {
            var directories = ExplorerHelper.GetOpenedDirectories();
            ExplorerHelper.KillExplorer();
            ExplorerHelper.OpenDirectories(directories);
        }
    }
}
