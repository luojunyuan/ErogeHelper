using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Extensions;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Dialogs;
using Newtonsoft.Json;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vanara.PInvoke.NetListMgr;

namespace ErogeHelper.Model.Services
{
    public class SavedataSyncService : ISavedataSyncService, IEnableLogger
    {
        private readonly IGameDataService _gameDataService;
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IEhConfigRepository _ehConfigDataService;
        private readonly INetworkListManager _networkListManager;
        private bool IsNetConnected => _networkListManager.IsConnectedToInternet;
        private string GameMd5 => _gameDataService.Md5;
        private string LocalSavedataFolder => _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
        private string CloudSavedataFolder => Path.Combine(
            _ehConfigDataService.ExternalSharedDrivePath, Path.GetFileNameWithoutExtension(LocalSavedataFolder));
        private string CloudDbFilePath => Path.Combine(
            _ehConfigDataService.ExternalSharedDrivePath, ConstantValues.EhCloudDbFilename);

        public string[] ExcludeFiles { get; }

        public SavedataSyncService(
            IGameDataService? gameDataService = null,
            IEhDbRepository? ehDbRepository = null,
            IEhConfigRepository? ehConfigDataService = null,
            INetworkListManager? networkListManager = null)
        {
            _gameDataService = gameDataService ?? DependencyInject.GetService<IGameDataService>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
            _networkListManager = networkListManager ?? DependencyInject.GetService<INetworkListManager>();

            ExcludeFiles = GetCurrentGameData()?.ExcludeFiles ?? Array.Empty<string>();
        }

        public void InitGameData()
        {
            var cloudGameDatas = File.Exists(CloudDbFilePath) ?
                JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath)) :
                new();

            var currentData = CreateGameData();
            cloudGameDatas.Add(currentData);
            Directory.CreateDirectory(CloudSavedataFolder);
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

        public CloudSaveDataEntity CreateGameData() =>
            new(GameMd5,
                LocalSavedataFolder,
                GetLastDirectoryModifiedTime(LocalSavedataFolder),
                ExcludeFiles,
                Environment.MachineName,
                Utils.MachineGuid);

        /// <summary>
        /// Get savedata info from cloud
        /// </summary>
        public CloudSaveDataEntity? GetCurrentGameData() =>
            !File.Exists(CloudDbFilePath)
                ? null
                : JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
                    .FirstOrDefault(g => g.Md5.Equals(GameMd5, StringComparison.Ordinal));

        public void DownloadSync()
        {
            try
            {
                if (!IsNetConnected)
                {
                    return;
                }

                if (!Directory.Exists(CloudSavedataFolder) || !Directory.Exists(LocalSavedataFolder))
                {
                    _ehDbRepository.UpdateGameInfo(_ehDbRepository.GameInfo! with
                    {
                        SavedataPath = string.Empty,
                        UseCloudSave = false
                    });
                    return;
                }

                var localSaveTime = GetLastDirectoryModifiedTime(LocalSavedataFolder);
                var cloudSaveTime = GetLastDirectoryModifiedTime(CloudSavedataFolder);

                CloudSaveDataEntity gamedata = GetCurrentGameData() ?? throw new ArgumentNullException(nameof(gamedata));

                if (!gamedata.PCId.Equals(Utils.MachineGuid, StringComparison.Ordinal))
                {
                    if (cloudSaveTime < localSaveTime)
                    {
                        var overWritten = new SavedataConflictDialog().ShowDialog();
                        if (overWritten == true)
                        {
                            DownloadFiles();
                            UpdateSavedataInfo();
                        }
                    }
                    else
                    {
                        DownloadFiles();
                    }
                }
            }
            catch (IOException ex)
            {
                this.Log().Debug(ex.Message);
            }
            catch (Exception ex)
            {
                this.Log().Debug(ex);
            }
        }

        public void UpdateSync()
        {
            try
            {
                if (!IsNetConnected)
                {
                    return;
                }

                CloudSaveDataEntity gamedata = GetCurrentGameData() ?? throw new ArgumentNullException(nameof(gamedata));

                var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
                var saveTime = GetLastDirectoryModifiedTime(savedataPath);
                if (gamedata.PCId.Equals(Utils.MachineGuid, StringComparison.Ordinal) && gamedata.LastTimeModified < saveTime)
                {
                    UploadFiles();
                    UpdateLastModifiedTime(saveTime);
                }
                else if (!gamedata.PCId.Equals(Utils.MachineGuid, StringComparison.Ordinal))
                {
                    var result = ModernWpf.MessageBox.Show("Upload savedata to cloud?", "Warning", System.Windows.MessageBoxButton.YesNo);
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        UploadFiles();
                        UpdateSavedataInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log().Debug(ex);
            };
        }

        private void UpdateSavedataInfo()
        {
            CloudDbFilePath.CheckFileExist();

            var cloudGameDatas =
                JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
                ?? new();

            var newData = CreateGameData();

            cloudGameDatas.RemoveAll(item => item.Md5.Equals(newData.Md5, StringComparison.Ordinal));
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

            cloudGameDatas.RemoveAll(item => item.Md5.Equals(currentData.Md5, StringComparison.Ordinal));
            cloudGameDatas.Add(currentData with { LastTimeModified = time });
            File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));
        }

        private void DownloadFiles() => DirectoryCopy(CloudSavedataFolder, LocalSavedataFolder, true, true);

        private void UploadFiles() => DirectoryCopy(LocalSavedataFolder, CloudSavedataFolder, true, true);

        private static DateTime GetLastDirectoryModifiedTime(string path) =>
            new DirectoryInfo(path)
                .EnumerateFileSystemInfos()
                .Max(i => i.LastWriteTime);

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
        {
            // 不需要的文件
            // md5相同的文件 变化的
            DirectoryInfo dir = new(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overwrite);
            }

            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overwrite);
                }
            }
        }
    }
}
