using ErogeHelper.Common;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel.Windows
{
    public class PreferenceViewModel : ReactiveObject, IEnableLogger
    {
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IEhConfigDataService _ehConfigDataService;
        private readonly ISavedataSyncService _savedataSyncService;
        private bool _cloudSwitchIsOn;

        public PreferenceViewModel(
            IEhDbRepository? ehDbRepository = null,
            IEhConfigDataService? ehConfigDataService = null,
            ISavedataSyncService? savedataSyncService = null)
        {
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigDataService>();
            _savedataSyncService = savedataSyncService ?? DependencyInject.GetService<ISavedataSyncService>();

            var savedataSyncDialog = new SavedataSyncDialog(_ehConfigDataService, ehDbRepository);
            if (savedataSyncDialog.UNCPath.Text != string.Empty &&
                savedataSyncDialog.SavedataPath.Text != string.Empty)
            {
                CloudSwitchCanBeSet = true;
                _cloudSwitchIsOn = _ehDbRepository.GameInfo?.UseCloudSave ?? false;
            }

            OpenCloudEditDialog = ReactiveCommand.CreateFromTask(async () =>
            {
                await savedataSyncDialog.ShowAsync();
                if (savedataSyncDialog.UNCPath.Text != string.Empty &&
                    savedataSyncDialog.SavedataPath.Text != string.Empty)
                {
                    CloudSwitchCanBeSet = true;
                }
            });
        }

        [Reactive]
        public bool CloudSwitchCanBeSet { get; set; }

        // TODO: Use reactive way like property
        public bool CloudSwitchIsOn
        {
            get => _cloudSwitchIsOn;
            set
            {
                _cloudSwitchIsOn = value;
                this.RaiseAndSetIfChanged(ref _cloudSwitchIsOn, value);
                _ehDbRepository.UpdateCloudStatus(value);

                if (value)
                {
                    // first time
                    var savedataInfo = _savedataSyncService.GetCurrentGameData();
                    if (savedataInfo == null)
                    {
                        _savedataSyncService.InitGameData();
                        Task.Run(_savedataSyncService.UploadFiles);
                    }
                }
            }
        }

        public ReactiveCommand<Unit, Unit> OpenCloudEditDialog { get; set; }

        //public SavedataSyncViewModel()
        //{
        //    KeyAction = ReactiveCommand.Create(() => 
        //    {
        //        var md5 = DependencyInject.GetService<IGameDataService>().Md5;
        //        var repo = DependencyInject.GetService<IEhDbRepository>();

        //        var config = DependencyInject.GetService<IEHConfigDataService>();
        //        config.ExternalSharedDrivePath = CloudPath;

        //        var newInfo = repo.GetGameInfo(md5)!;
        //        newInfo.CloudPath = SaveDataPath;
        //        newInfo.UseCloudSave = true;
        //        repo.UpdateGameInfo(newInfo);
        //        var nucDic = Path.Combine(CloudPath, "eh-cloud-savedata", Path.GetFileName(SaveDataPath));
        //        DirectoryCopy(SaveDataPath, nucDic, true, true);

        //        // 写入很多相关的文件
        //        // 打开文件检测的服务
        //    });


   
    }
}
