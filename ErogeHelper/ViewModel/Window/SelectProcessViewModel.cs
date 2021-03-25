using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Interface;
using ErogeHelper.Model.Service.Interface;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using ErogeHelper.Model.Repository.Entity;

namespace ErogeHelper.ViewModel.Window
{
    public class SelectProcessViewModel : PropertyChangedBase
    {
        public SelectProcessViewModel(
            ISelectProcessDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            IGameWindowHooker gameWindowHooker,
            IEhServerApi ehServerApi,
            EhConfigRepository ehConfigRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _gameWindowHooker = gameWindowHooker;
            _ehServerApi = ehServerApi;
            // _ehDatabase = ehDatabase
            _ehConfigRepository = ehConfigRepository;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IEhServerApi _ehServerApi;
        private readonly EhConfigRepository _ehConfigRepository;

        public BindableCollection<ProcComboBoxItem> ProcItems { get; } = new();

        private ProcComboBoxItem? _selectedProcItem;
        public ProcComboBoxItem? SelectedProcItem
        {
            get => _selectedProcItem;
            set
            {
                _selectedProcItem = value;
                NotifyOfPropertyChange(() => CanInject);
            }
        }

        public async void GetProcessAction() =>
            await _dataService.RefreshBindableProcComboBoxAsync(ProcItems).ConfigureAwait(false);

        public bool CanInject => SelectedProcItem is not null;

        public async void Inject()
        {
            if (SelectedProcItem!.Proc.HasExited)
            {
                await _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(GetType(), ViewAction.OpenDialog, ModernDialog.SelectProcessNoProcessTip))
                    .ConfigureAwait(false);
                // Cause this turn SelectedProcItem to null
                // NOTE: 此时用户来个手速操作，ProcItems.Remove(null)，无事发生
                ProcItems.Remove(SelectedProcItem);
                return;
            }

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Hide));

            (_ehConfigRepository.GameProcesses, _ehConfigRepository.MainProcess) =
                Utils.ProcessCollect(SelectedProcItem.Proc.ProcessName);

            var md5 = Utils.GetFileMd5(_ehConfigRepository.MainProcess.MainModule?.FileName ?? string.Empty);

            var settingJson = string.Empty;
            // 先使用dapper获取本地有没有对应md5的游戏信息
            // 一大帕拉东西找json
            try
            {
                using var resp = await _ehServerApi.GetGameSetting(md5).ConfigureAwait(true);
                // For throw ApiException
                //resp = await resp.EnsureSuccessStatusCodeAsync();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var gameInfo = resp.Content ?? new GameSetting();
                    settingJson = gameInfo.GameSettingJson;
                }
                Log.Debug($"{resp.StatusCode} {resp.Content}");
            }
            catch (HttpRequestException ex)
            {
                Log.Warn("Can't connect to internet", ex);
            }

            if (settingJson == string.Empty)
            {
                await _windowManager.ShowWindowFromIoCAsync<HookConfigViewModel>().ConfigureAwait(false);
                _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
                return;
            }

            var gameSetting = JsonSerializer.Deserialize<GameTextSetting>(settingJson) ??
                              new GameTextSetting();
            gameSetting.Md5 = md5;
            _ehConfigRepository.TextractorSetting = gameSetting;
            _textractorService.InjectProcesses();

            // NOTE: WindowManger每次都会创建新窗口，之后再调相同的VM的话就会创建新的窗口，所以必须通过VM或Message来操作相应View
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            _ = _gameWindowHooker.SetGameWindowHookAsync();

            _ = _ehConfigRepository.UseOutsideWindow
                ? _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"))
                : _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
        }

        #pragma warning disable 8618
        public SelectProcessViewModel() { }
    }
}