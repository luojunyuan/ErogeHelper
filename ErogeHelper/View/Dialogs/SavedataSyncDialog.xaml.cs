using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;

namespace ErogeHelper.View.Dialogs
{
    /// <summary>
    /// SavedataSyncDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SavedataSyncDialog
    {
        private const string UNCPathTip = "Please select a folder. EH would create a directory in it.";
        private const string SavedataPathTip = @"Please select game savedata folder. for example ""E:\k1mlka\Documents\AliceSoft\ドーナドーナ いっしょにわるいことをしよう""";

        private readonly IEhConfigDataService _eHConfigDataService;
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IGameDataService _gameDataService;

        public SavedataSyncDialog(
            IEhConfigDataService? ehConfigDataService = null,
            IEhDbRepository? ehDbRepository = null,
            IGameDataService? gameDataService = null)
        {
            InitializeComponent();

            _eHConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();

            UNCPath.Text = _eHConfigDataService.ExternalSharedDrivePath;
            SavedataPath.Text = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
        }

        private void UNCSetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var path = ShowFolderBrowserDialog(string.Empty, UNCPathTip);
            if (path != string.Empty)
            {
                var ehSavedata = Path.Combine(path, ConstantValues.EhCloudSavedataTag);
                Directory.CreateDirectory(ehSavedata);
                UNCPath.Text = ehSavedata;
                _eHConfigDataService.ExternalSharedDrivePath = ehSavedata;
            }
        }

        private void SavedataRomingSetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var path = ShowFolderBrowserDialog(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SavedataPathTip);
            if (path != string.Empty)
            {
                SavedataPath.Text = path;
                _ehDbRepository.UpdateSavedataPath(path);
            }
        }

        private void SavedataDocumentsSetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var path = ShowFolderBrowserDialog(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SavedataPathTip);
            if (path != string.Empty)
            {
                SavedataPath.Text = path;
                _ehDbRepository.UpdateSavedataPath(path);
            }
        }

        private void SavedataGameDirectorySetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var path = ShowFolderBrowserDialog(
                Path.GetDirectoryName(_gameDataService.GamePath) ?? string.Empty, SavedataPathTip);
            if (path != string.Empty)
            {
                SavedataPath.Text = path;
                _ehDbRepository.UpdateSavedataPath(path);
            }
        }

        private static string ShowFolderBrowserDialog(string rootFolder, string description)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true,
                SelectedPath = rootFolder + "\\",
            };

            if (dialog.ShowDialog() ?? false)
            {
                return dialog.SelectedPath;
            }

            return string.Empty;
        }
    }
}
