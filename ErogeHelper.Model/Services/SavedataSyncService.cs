using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using Newtonsoft.Json;
using Vanara.PInvoke;

namespace ErogeHelper.Model.Services;

public class SavedataSyncService : ISavedataSyncService
{
    public static Func<bool> MessageBox { get; set; } = null!;

    private string FolderName => Path.GetFileNameWithoutExtension(LocalSavedataFolder);
    private string LocalSavedataFolder => _gameInfoRepository.GameInfo.SaveDataPath;
    private string CloudGameSaveFolder => Path.Combine(
        _ehConfigRepository.ExternalSharedDrivePath, FolderName);
    private string CloudDBFilePath => Path.Combine(
        _ehConfigRepository.ExternalSharedDrivePath, ConstantValue.CloudDbFilename);

    private readonly IGameInfoRepository _gameInfoRepository;
    private readonly IEHConfigRepository _ehConfigRepository;

    public SavedataSyncService(
        IGameInfoRepository? gameInfoRepository = null,
        IEHConfigRepository? ehConfigRepository = null)
    {
        _gameInfoRepository = gameInfoRepository ?? Shared.DependencyResolver.GetService<IGameInfoRepository>();
        _ehConfigRepository = ehConfigRepository ?? Shared.DependencyResolver.GetService<IEHConfigRepository>();
    }

    // 打开设置switch上传时
    // 云端没有 就上传当前存档
    // 云端有了 -> 对比。。。

    // 游戏启动时
    // 连不上网 None
    // 找不到数据库|找不到游戏存档 throw Toast提示
    // 连得上网 -> 对比。。。

    // 游戏结束时
    // 连不上网 None
    // 找不到数据库|找不到游戏存档 throw 弹窗提示或直接报错
    // 连得上网 -> 对比。。。

    // 对比时需要询问用户输入，使用 回调 或 IObservable 或 Interaction

    //public void UpdateSync()
    //{
    //    if (!WinINet.InternetGetConnectedState(out _))
    //    {
    //        return;
    //    }
    //    var cloadDBFilePath = CloudDBFilePath;
    //    if (File.Exists(cloadDBFilePath))
    //    {
    //        throw new FileNotFoundException(cloadDBFilePath);
    //    }
    //    var localSavedataFolder = LocalSavedataFolder;
    //    if (File.Exists(localSavedataFolder))
    //    {
    //        throw new FileNotFoundException(localSavedataFolder);
    //    }


    //    var gamedata = GetCurrentGameData(FolderName, cloadDBFilePath);

    //    if (gamedata is null)
    //    {
    //        // 没有数据库 或 当前游戏信息
    //        // first time
    //        return;
    //    }

    //    var guid = DependencyResolver.GetService<IWindowsGuid>().MachineGuid;
    //    var saveTime = GetLastDirectoryModifiedTime(localSavedataFolder);
    //    if (gamedata.PCId.Equals(guid, StringComparison.Ordinal) && gamedata.LastTimeModified < saveTime)
    //    {
    //        UploadFiles(localSavedataFolder, CloudGameSaveFolder);
    //        UpdateLastModifiedTime(saveTime);
    //    }
    //    else if (!gamedata.PCId.Equals(guid, StringComparison.Ordinal))
    //    {
    //        var result = MessageBox();
    //        if (result == true)
    //        {
    //            UploadFiles(localSavedataFolder, CloudGameSaveFolder);
    //            UpdateSavedataInfo();
    //        }
    //    }
    //}


    ////public void InitGameData()
    ////{
    ////    var cloudGameDatas = File.Exists(CloudDbFilePath) ?
    ////        JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath)) :
    ////        new();

    ////    var currentData = CreateGameData();
    ////    cloudGameDatas.Add(currentData);
    ////    Directory.CreateDirectory(CloudSavedataFolder);
    ////    File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));

    ////    Task.Run(UploadFiles);

    ////    //if (File.Exists(cloudDbFile))
    ////    //{
    ////    //    var cloudGameDatas =
    ////    //        JsonConvert.DeserializeObject<List<CloudGameDataEntity>>(File.ReadAllText(cloudDbFile))
    ////    //        .CheckNull();

    ////    //    var currentData = CreateGameData();
    ////    //    cloudGameDatas.Add(currentData);

    ////    //    File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
    ////    //}
    ////    //else
    ////    //{
    ////    //    var currentData = CreateGameData();
    ////    //    var cloudGameDatas = new List<CloudGameDataEntity> { currentData };

    ////    //    File.WriteAllText(cloudDbFile, JsonConvert.SerializeObject(cloudGameDatas));
    ////    //}
    ////}

    //public CloudSaveDataTerm CreateGameData() =>
    //    new(FolderName,
    //        LocalSavedataFolder,
    //        GetLastDirectoryModifiedTime(LocalSavedataFolder),
    //        ExcludeFiles,
    //        Environment.MachineName,
    //        Utils.MachineGuid);

    ///// <summary>
    ///// Get savedata info from cloud
    ///// </summary>
    //// TODO: Use System.Text.Json
    //private static CloudSaveDataTerm? GetCurrentGameData(string folderName, string cloudDBFilePath) => 
    //    JsonConvert.DeserializeObject<List<CloudSaveDataTerm>>(File.ReadAllText(cloudDBFilePath))
    //        ?.FirstOrDefault(g => g.FolderName.Equals(folderName, StringComparison.Ordinal));

    //private static DateTime GetLastDirectoryModifiedTime(string path) =>
    //    new DirectoryInfo(path)
    //        .EnumerateFileSystemInfos()
    //        .Max(i => i.LastWriteTime);

    //private static void UploadFiles(string localSavedataFolder, string cloudSavedataFolder) => 
    //    DirectoryCopy(localSavedataFolder, cloudSavedataFolder, true, true);

    //private void UpdateLastModifiedTime(string cloudDBFilePath, DateTime time)
    //{
    //    var cloudGameDatas =
    //        JsonConvert.DeserializeObject<List<CloudSaveDataTerm>>(File.ReadAllText(cloudDBFilePath))
    //        ?? new();

    //    var currentData = CreateGameData();

    //    cloudGameDatas.RemoveAll(item => item.Md5.Equals(currentData.Md5, StringComparison.Ordinal));
    //    cloudGameDatas.Add(currentData with { LastTimeModified = time });
    //    File.WriteAllText(cloudDBFilePath, JsonConvert.SerializeObject(cloudGameDatas));
    //}

    //private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
    //{
    //    // 不需要的文件
    //    // md5相同的文件 变化的
    //    DirectoryInfo dir = new(sourceDirName);

    //    if (!dir.Exists)
    //    {
    //        throw new DirectoryNotFoundException(
    //            "Source directory does not exist or could not be found: "
    //            + sourceDirName);
    //    }

    //    var dirs = dir.GetDirectories();

    //    Directory.CreateDirectory(destDirName);

    //    var files = dir.GetFiles();
    //    foreach (var file in files)
    //    {
    //        var tempPath = Path.Combine(destDirName, file.Name);
    //        file.CopyTo(tempPath, overwrite);
    //    }

    //    if (copySubDirs)
    //    {
    //        foreach (var subdir in dirs)
    //        {
    //            var tempPath = Path.Combine(destDirName, subdir.Name);
    //            DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overwrite);
    //        }
    //    }
    //}

    //public void DownloadSync()
    //{
    //    try
    //    {
    //        if (!IsNetConnected)
    //        {
    //            return;
    //        }

    //        if (!Directory.Exists(CloudSavedataFolder) || !Directory.Exists(LocalSavedataFolder))
    //        {
    //            _ehDbRepository.UpdateGameInfo(_ehDbRepository.GameInfo! with
    //            {
    //                SavedataPath = string.Empty,
    //                UseCloudSave = false
    //            });
    //            return;
    //        }

    //        var localSaveTime = GetLastDirectoryModifiedTime(LocalSavedataFolder);
    //        var cloudSaveTime = GetLastDirectoryModifiedTime(CloudSavedataFolder);

    //        CloudSaveDataEntity gamedata = GetCurrentGameData() ?? throw new ArgumentNullException(nameof(gamedata));

    //        if (!gamedata.PCId.Equals(Utils.MachineGuid, StringComparison.Ordinal))
    //        {
    //            if (cloudSaveTime < localSaveTime)
    //            {
    //                var overWritten = new SavedataConflictDialog().ShowDialog();
    //                if (overWritten == true)
    //                {
    //                    DownloadFiles();
    //                    UpdateSavedataInfo();
    //                }
    //            }
    //            else
    //            {
    //                DownloadFiles();
    //            }
    //        }
    //    }
    //    catch (IOException ex)
    //    {
    //        this.Log().Debug(ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        this.Log().Debug(ex);
    //    }
    //}

    //private void UpdateSavedataInfo()
    //{
    //    CloudDbFilePath.CheckFileExist();

    //    var cloudGameDatas =
    //        JsonConvert.DeserializeObject<List<CloudSaveDataEntity>>(File.ReadAllText(CloudDbFilePath))
    //        ?? new();

    //    var newData = CreateGameData();

    //    cloudGameDatas.RemoveAll(item => item.Md5.Equals(newData.Md5, StringComparison.Ordinal));
    //    cloudGameDatas.Add(newData);
    //    File.WriteAllText(CloudDbFilePath, JsonConvert.SerializeObject(cloudGameDatas));
    //}


    //private void DownloadFiles() => DirectoryCopy(CloudSavedataFolder, LocalSavedataFolder, true, true);

}
