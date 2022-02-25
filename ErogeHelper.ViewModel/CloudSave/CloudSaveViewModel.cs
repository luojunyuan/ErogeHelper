using System.Reactive;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using ErogeHelper.Shared.Contracts;
using Vanara.PInvoke;
using ErogeHelper.Model.Services.Interface;

namespace ErogeHelper.ViewModel.CloudSave;

public class CloudSaveViewModel : ReactiveObject
{
    private const string UNCPathTip = "Please select a folder. EH would create a directory in it.";
    private const string SavedataPathTip = 
        @"Please select game savedata folder. for example ""E:\k1mlka\Documents\AliceSoft\ドーナドーナ いっしょにわるいことをしよう""";
    private const string ConflictDialogTip = 
        "Sync savedata from cloud immedtely, otherwise eh will upload file to cloud after game exit.";

    public CloudSaveViewModel(
        IGameInfoRepository? gameInfoRepository = null,
        IEHConfigRepository? ehConfigRepository = null,
        IGameDataService? gameDataService = null)
    {
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();

        UNCDatabasePath = ehConfigRepository.ExternalSharedDrivePath;
        GameSavedataPath = gameInfoRepository.GameInfo.SaveDataPath;
        // TODO: How about folder not exist
        ShowNoInternet = WinINet.InternetGetConnectedState(out _);
        CanEnable = ShowNoInternet && UNCDatabasePath != string.Empty && GameSavedataPath != string.Empty;
        IsSwitchOn = gameInfoRepository.GameInfo.UseCloudSave;

        SetUNCPath = ReactiveCommand.CreateFromObservable(() => 
            Interactions.FolderBrowserDialog.Handle((string.Empty, UNCPathTip)));

        SetRomingPath = ReactiveCommand.CreateFromObservable(() => Interactions.FolderBrowserDialog.Handle(
            (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SavedataPathTip)));
        SetDocumentsPath = ReactiveCommand.CreateFromObservable(() => Interactions.FolderBrowserDialog.Handle(
            (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SavedataPathTip)));
        SetGameSaveFolderPath = ReactiveCommand.CreateFromObservable(() => Interactions.FolderBrowserDialog.Handle(
            (Path.GetDirectoryName(gameDataService.GamePath) ?? string.Empty, SavedataPathTip)));

        SetUNCPath
            // TODO: Tip only Path begin with \\ like \\192.168.0.1\Folder
            .Where(path => path.StartsWith("\\\\"))
            .Select(path => Path.Combine(path, ConstantValue.CloudSaveDataTag))
            .Subscribe(db =>
            {
                UNCDatabasePath = db;
                Directory.CreateDirectory(db);
                ehConfigRepository.ExternalSharedDrivePath = db;
            });

        SetRomingPath
            .Merge(SetDocumentsPath)
            .Merge(SetGameSaveFolderPath)
            .Where(path => path != string.Empty)
            .Subscribe(path =>
            {
                GameSavedataPath = path;
                gameInfoRepository.UpdateSavedataPath(path);
            });

        this.WhenAnyValue(x => x.UNCDatabasePath, x => x.GameSavedataPath,
            (a, b) => a != string.Empty && b != string.Empty)
            .Subscribe(hasPath => CanEnable = hasPath);

        this.WhenAnyValue(x => x.IsSwitchOn)
            .Subscribe(useCloudsave => gameInfoRepository.UpdateCloudStatus(useCloudsave));
        //_ehDbRepository.UpdateCloudStatus(value);
        //if (value)
        //{
        //    var savedataInfo = _savedataSyncService.GetCurrentGameData();
        //    if (savedataInfo is null)
        //    {
        //        _savedataSyncService.InitGameData();
        //    }
        //    else if (!savedataInfo.PCId.Equals(Utils.MachineGuid, System.StringComparison.Ordinal))
        //    {
        //        var result = ModernWpf.MessageBox.Show("Sync savedata", "Warning", System.Windows.MessageBoxButton.YesNo);
        //        if (result == System.Windows.MessageBoxResult.Yes)
        //        {
        //            _savedataSyncService.DownloadSync();
        //        }
        //    }
        //}
    }

    [Reactive]
    public string UNCDatabasePath { get; set; }

    public ReactiveCommand<Unit, string> SetUNCPath { get; }

    public ReactiveCommand<Unit, string> SetRomingPath { get; }
    public ReactiveCommand<Unit, string> SetDocumentsPath { get; }
    public ReactiveCommand<Unit, string> SetGameSaveFolderPath { get; }

    [Reactive]
    public string GameSavedataPath { get; set; }

    [Reactive]
    public bool CanEnable { get; set; }

    [Reactive]
    public bool ShowNoInternet { get; set; }

    [Reactive]
    public bool IsSwitchOn { get; set; }
}
