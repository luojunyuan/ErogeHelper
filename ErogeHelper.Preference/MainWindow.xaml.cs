using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
            OldScreenShot.IsChecked = bool.Parse( config.Read("ScreenShotTradition") ?? "false");
            ZtwoEnter.IsChecked     = bool.Parse( config.Read("EnterKeyMapping") ?? "false");

            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
        }

        private void OldScreenShot_Click(object sender, RoutedEventArgs e)
        {
            var config = new IniFile(ConfigFilePath);
            config.Write("ScreenShotTradition", OldScreenShot.IsChecked.ToString());
        }

        private void ZtwoEnter_Click(object sender, RoutedEventArgs e)
        {
            var config = new IniFile(ConfigFilePath);
            config.Write("EnterKeyMapping", OldScreenShot.IsChecked.ToString());
        }
    }
}
