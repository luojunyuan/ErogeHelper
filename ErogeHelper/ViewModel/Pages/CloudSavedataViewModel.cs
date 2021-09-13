using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.View.Dialogs;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;

namespace ErogeHelper.ViewModel.Pages
{
    public class CloudSavedataViewModel : ReactiveObject, IRoutableViewModel, IEnableLogger
    {
        private readonly IEhDbRepository _ehDbRepository;
        private readonly IEhConfigRepository _ehConfigDataService;
        private readonly ISavedataSyncService _savedataSyncService;
        private bool _cloudSwitchIsOn;

        public CloudSavedataViewModel(
            IPreferenceScreen? hostScreen = null,
            IEhDbRepository? ehDbRepository = null,
            IEhConfigRepository? ehConfigDataService = null,
            ISavedataSyncService? savedataSyncService = null)
        {
            HostScreen = hostScreen ?? DependencyInject.GetService<IPreferenceScreen>();
            _ehDbRepository = ehDbRepository ?? DependencyInject.GetService<IEhDbRepository>();
            _ehConfigDataService = ehConfigDataService ?? DependencyInject.GetService<IEhConfigRepository>();
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
        }

        public ReactiveCommand<Unit, Unit> OpenCloudEditDialog { get; }

        public string? UrlPathSegment => PageTags.General;

        public IScreen HostScreen { get; }
    }
}
