using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Extensions;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Services
{
    public class SavedataSyncService : ISavedataSyncService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IEhConfigDataService _ehConfigDataService;
        private string GameMd5 => _gameDataService.Md5;
        private string LocalSavedataPath => _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
        private string CloudSavedataDirectory => Path.Combine(
            _ehConfigDataService.ExternalSharedDrivePath, Path.GetFileNameWithoutExtension(LocalSavedataPath));
        private string CloudDbFilePath => Path.Combine(
            _ehConfigDataService.ExternalSharedDrivePath, ConstantValues.EhCloudDbFilename);

        public SavedataSyncService(
            IGameDataService? gameDataService = null,
            IEhDbRepository? ehDbRepository = null,
            IEhConfigDataService? ehConfigDataService = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigDataService>();
        }

        public void InitGameData()
        {
            var cloudGameDatas = File.Exists(CloudDbFilePath) ? 
                JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath)) :
                new();

            var currentData = CreateGameData();
            cloudGameDatas.Add(currentData);
            Directory.CreateDirectory(CloudSavedataDirectory);
            File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));

            Task.Run(UploadFiles);

            //if (File.Exists(cloudDbFile))
            //{
            //    var cloudGameDatas =
            //        JsonConvert.DeserializeObject<List<CloudGameDataEntity>>(File.ReadAllText(cloudDbFile))
            //        .CheckNull();

            //    var currentData = CreateGameData();
            //    cloudGameDatas.Add(currentData);

            //    File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
            //}
            //else
            //{
            //    var currentData = CreateGameData();
            //    var cloudGameDatas = new List<CloudGameDataEntity> { currentData };

            //    File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
            //}
        }

        // Add filterFiles
        public CloudSaveDataEntity CreateGameData() =>
            new(GameMd5,
                LocalSavedataPath,
                GetLastDirectoryModifiedTime(LocalSavedataPath),
                Environment.MachineName,
                Utils.MachineGUID);

        public CloudSaveDataEntity? GetCurrentGameData()
        {
            if (!File.Exists(CloudDbFilePath))
                return null;

            return JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
                .FirstOrDefault(g => g.Md5.Equals(GameMd5, StringComparison.Ordinal));
        }

        public void DownloadSync()
        {
            var localSaveTime = GetLastDirectoryModifiedTime(LocalSavedataPath);

            // Side effect
            if (!Directory.Exists(CloudSavedataDirectory))
            {
                var gameInfo = _ehDbRepository.GameInfo!;
                gameInfo.SavedataPath = string.Empty;
                gameInfo.UseCloudSave = false;
                _ehDbRepository.UpdateGameInfo(gameInfo);
                return;
            }

            var cloudSaveTime = GetLastDirectoryModifiedTime(CloudSavedataDirectory);

            var gamedata = GetCurrentGameData().CheckNull();

            // main
            if (!gamedata.PCId.Equals(Utils.MachineGUID))
            {
                if (cloudSaveTime < localSaveTime)
                {
                    var result = ModernWpf.MessageBox.Show("Sync savedata from cloud immedtely, otherwise eh will upload file to cloud after game exit.", "Warning", System.Windows.MessageBoxButton.YesNo);
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        DownloadFiles();
                        //
                        UpdateSavedataInfo();
                    }
                }
                else
                {
                    DownloadFiles();
                }
            }
            // Consider about the path may be changed
        }

        public void UpdateSync()
        {
            var gamedata = GetCurrentGameData().CheckNull();

            var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
            var saveTime = GetLastDirectoryModifiedTime(savedataPath);
            if (gamedata.PCId.Equals(Utils.MachineGUID) && gamedata.LastTimeModified < saveTime)
            {
                UploadFiles();
                UpdateLastModifiedTime(saveTime);
            }
            else if (!gamedata.PCId.Equals(Utils.MachineGUID))
            {
                var result = ModernWpf.MessageBox.Show("Upload savedata to cloud?", "Warning", System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    UploadFiles();
                    UpdateSavedataInfo();
                }
            }
        }

        private void UpdateSavedataInfo()
        {
            CloudDbFilePath.CheckFileExist();
            var cloudGameDatas =
                JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
                ?? new();

            var newData = CreateGameData();

            cloudGameDatas.RemoveAll(item => item.Md5.Equals(newData.Md5));
            cloudGameDatas.Add(newData);
            File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));
        }

        private void UpdateLastModifiedTime(DateTime time)
        {
            CloudDbFilePath.CheckFileExist();
            var cloudGameDatas =
                JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
                ?? new();

            var currentData = CreateGameData();

            cloudGameDatas.RemoveAll(item => item.Md5.Equals(currentData.Md5));
            cloudGameDatas.Add(currentData with { LastTimeModified = time });
            File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));
        }

        private void DownloadFiles() => DirectoryCopy(CloudSavedataDirectory, LocalSavedataPath, true, true);

        private void UploadFiles() => DirectoryCopy(LocalSavedataPath, CloudSavedataDirectory, true, true);

        private static DateTime GetLastDirectoryModifiedTime(string path) =>
            new DirectoryInfo(path)
                .EnumerateFileSystemInfos()
                .Max(i => i.LastWriteTime);

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
