using ErogeHelper.Common;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace ErogeHelper.ViewModel.Windows
{
    public class SavedataSyncViewModel : ReactiveObject, IEnableLogger
    {
        public SavedataSyncViewModel()
        {
            KeyAction = ReactiveCommand.Create(() => 
            {
                var md5 = DependencyInject.GetService<IGameDataService>().Md5;
                var repo = DependencyInject.GetService<IEhDbRepository>();

                var config = DependencyInject.GetService<IEHConfigDataService>();
                config.ExternalSharedDrivePath = CloudPath;

                var newInfo = repo.GetGameInfo(md5)!;
                newInfo.CloudPath = SaveDataPath;
                newInfo.UseCloudSave = true;
                repo.UpdateGameInfo(newInfo);
                var nucDic = Path.Combine(CloudPath, "eh-cloud-savedata", Path.GetFileName(SaveDataPath));
                DirectoryCopy(SaveDataPath, nucDic, true, true);

                // 写入很多相关的文件
                // 打开文件检测的服务
            });

            SelectCloudPath = ReactiveCommand.Create(() =>
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Please select a folder.";
                dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

                if ((bool)dialog.ShowDialog())
                {
                    CloudPath = dialog.SelectedPath;
                    if (!string.IsNullOrWhiteSpace(CloudPath) && !string.IsNullOrWhiteSpace(SaveDataPath))
                    {
                        CloudSwitchCanBeOpen = true;
                    }
                }
            });

            SelectSaveDataPath = ReactiveCommand.Create(() => 
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Please select a folder.";
                dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

                if ((bool)dialog.ShowDialog())
                {
                    SaveDataPath = dialog.SelectedPath;
                    if (!string.IsNullOrWhiteSpace(CloudPath) && !string.IsNullOrWhiteSpace(SaveDataPath))
                    {
                        CloudSwitchCanBeOpen = true;
                    }
                }

            });

            //var canOpen = this.WhenAnyValue(
            //    x => x.CloudPath, x => x.SaveDataPath,
            //    (cloudPath, saveDataPath) =>
            //        !string.IsNullOrWhiteSpace(cloudPath) &&
            //        !string.IsNullOrWhiteSpace(saveDataPath))
            //    .DistinctUntilChanged();

            CloudSwitch = ReactiveCommand.Create<bool>(status => 
            {
                this.Log().Debug(status);
            });
        }
        [Reactive]
        public bool CloudSwitchCanBeOpen { get; set; } = false;
        [Reactive]
        public string CloudPath { get; set; } = string.Empty;
        [Reactive]
        public string SaveDataPath { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> SelectCloudPath { get; set; }
        public ReactiveCommand<Unit, Unit> SelectSaveDataPath { get; set; }
        public ReactiveCommand<bool, Unit> CloudSwitch { get; set; }
        public ReactiveCommand<Unit, Unit> KeyAction { get; set; }

        private void ShowFolderBrowserDialog()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if ((bool)dialog.ShowDialog())
                CloudPath = dialog.SelectedPath;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
        {
            DirectoryInfo dir = new(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overwrite);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overwrite);
                }
            }
        }
    }
}
