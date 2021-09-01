using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DAL.Entity.Tables;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErogeHelper.Model.Services
{
    public class SavedataSyncService : ISavedataSyncService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IEhConfigDataService _ehConfigDataService;
        private readonly string MachineGUID = GetMachineGUID();
        private string GameMd5 => _gameDataService.Md5;

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
            var cloudDbFile = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, ConstantValues.EhCloudDbFilename);

            if (File.Exists(cloudDbFile))
            {
                var cloudGameDatas = JsonConvert.DeserializeObject<List<CloudGameDataEntity>>(File.ReadAllText(cloudDbFile));

                if (cloudGameDatas is null)
                {
                    throw new ArgumentNullException(nameof(cloudGameDatas));
                }

                var currentData = CreateGameData();
                cloudGameDatas.Add(currentData);

                File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
            }
            else
            {
                var currentData = CreateGameData();
                var cloudGameDatas = new List<CloudGameDataEntity>
                {
                    currentData
                };

                File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
            }
        }

        public CloudGameDataEntity CreateGameData()
        {
            var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
            return new CloudGameDataEntity(
                GameMd5,
                GetLastDirectoryModifiedTime(savedataPath),
                Environment.MachineName,
                MachineGUID);
        }

        public CloudGameDataEntity? GetCurrentGameData()
        {
            var cloudDbFile = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, ConstantValues.EhCloudDbFilename);
            if (!File.Exists(cloudDbFile))
            {
                return null;
            }

            var cloudGameDatas = JsonConvert.DeserializeObject<List<CloudGameDataEntity>>(File.ReadAllText(cloudDbFile));
            if (cloudGameDatas is null)
            {
                throw new ArgumentNullException(nameof(cloudGameDatas));
            }

            foreach (var gameData in cloudGameDatas)
            {
                if (gameData.Md5.Equals(GameMd5, StringComparison.Ordinal))
                {
                    return gameData;
                }
            }
            return null;
        }

        public void UpdateSaveInfo(DateTime time)
        {
            var cloudDbFile = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, ConstantValues.EhCloudDbFilename);
            var cloudGameDatas = JsonConvert.DeserializeObject<List<CloudGameDataEntity>>(File.ReadAllText(cloudDbFile));
       
            var gamedata = GetCurrentGameData();
            if (gamedata is null)
            {
                throw new ArgumentNullException(nameof(gamedata));
            }

            var newData = new CloudGameDataEntity
                (gamedata.Md5, time, Environment.MachineName, MachineGUID);

            cloudGameDatas.RemoveAll(item => item.Md5.Equals(gamedata.Md5));
            cloudGameDatas.Add(newData);
            File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
        }

        public void DownloadFiles()
        {
            var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
            var directoryName = Path.GetFileNameWithoutExtension(savedataPath);
            var cloudDirectory = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, directoryName);

            DirectoryCopy(cloudDirectory, savedataPath, true, true);
        }

        public void UploadFiles()
        {
            var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
            var directoryName = Path.GetFileNameWithoutExtension(savedataPath);
            var cloudDirectory = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, directoryName);

            DirectoryCopy(savedataPath, cloudDirectory, true, true);
        }

        private static DateTime GetLastDirectoryModifiedTime(string path) => 
            new DirectoryInfo(path)
                .EnumerateFileSystemInfos()
                .Max(i => i.LastWriteTime);

        private static string GetMachineGUID()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                var keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var keyX64 = keyBaseX64.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX64 = keyX64?.GetValue("MachineGuid", string.Empty);

                if (resultObjX64 is not null)
                {
                    return resultObjX64.ToString() ?? string.Empty;
                }
            }
            else
            {
                var keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var keyX86 = keyBaseX86.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
                var resultObjX86 = keyX86?.GetValue("MachineGuid", string.Empty);

                if (resultObjX86 != null)
                {
                    return resultObjX86.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
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

        public void DownloadSync(GameInfoTable gameInfo)
        {
            var savedataPath = gameInfo.SavedataPath ?? string.Empty;
            var localSaveTime = GetLastDirectoryModifiedTime(savedataPath);
            var directoryName = Path.GetFileNameWithoutExtension(savedataPath);
            var cloudDirectory = Path.Combine(
                _ehConfigDataService.ExternalSharedDrivePath, directoryName);
            if (!Directory.Exists(cloudDirectory))
            {
                gameInfo.SavedataPath = string.Empty;
                gameInfo.UseCloudSave = false;
                _ehDbRepository.UpdateGameInfo(gameInfo);
                return;
            }

            var cloudSaveTime = GetLastDirectoryModifiedTime(cloudDirectory);

            var gamedata = GetCurrentGameData();

            // 几种有限的情境，不会产生其他情形
            // 如果本地的档与云的一致，什么也不做。（上次退出游戏时已经更新到最新了）（标识肯是相同计算机不考虑其他情况）
            // 如果云的档比本地的旧，并且标识是相同计算机（说明上次退出游戏时没能成功更新到云），该。。。立即上传同步或者不管
            if (gamedata is not null && !gamedata.UniqueId.Equals(MachineGUID))
            {
                // 如果云的档比本地的旧，并且标识是不同计算机（说明断网情况下在本地游玩）(还可能是第一次加载。。)，需要弹窗提示用户手动替换。
                if (cloudSaveTime < localSaveTime)
                {
                    throw new NotImplementedException();
                }
                // 如果云的档比本地的新，并且标识是不同计算机，下载同步
                DownloadFiles();
            }
            else
            {

            }
            // 以上 断网 和“没有使用eh”意义是相同的
            // Exceptions
            // 云的档比本地的新，并且标识相同计算机（说明用户手动删除了存档），弹窗提示用户。
            // 断网情况，onedrive或nas有不同情境
            // 考虑游戏路径改变的情况，需要检查存档目录的存在状况
        }

        public void UpdateSync()
        {
            var gamedata = GetCurrentGameData();
            if (gamedata is null)
            {
                throw new ArgumentNullException(nameof(gamedata));
            }

            var savedataPath = _ehDbRepository.GameInfo?.SavedataPath ?? string.Empty;
            var saveTime = GetLastDirectoryModifiedTime(savedataPath);
            // 这个样子 在不同的计算机上 第一次设置并加载后会出问题
            // 如果是相同计算机 正确
            if (gamedata.UniqueId.Equals(MachineGUID) && gamedata.LastTimeModified < saveTime)
            {
                UploadFiles();
                UpdateSaveInfo(saveTime);
            }
            //  不同计算机，
        }
    }
}
