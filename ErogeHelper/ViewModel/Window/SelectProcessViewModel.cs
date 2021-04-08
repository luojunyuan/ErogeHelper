using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.Net;
using System.Net.Http;
using System.Text.Json;

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
            IEhServerApiService ehServerApiService,
            EhDbRepository ehDbRepository,
            EhConfigRepository ehConfigRepository,
            EhGlobalValueRepository ehGlobalValueRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _gameWindowHooker = gameWindowHooker;
            _ehServerApiService = ehServerApiService;
            _ehDbRepository = ehDbRepository;
            _ehConfigRepository = ehConfigRepository;
            _ehGlobalValueRepository = ehGlobalValueRepository;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IEhServerApiService _ehServerApiService;
        private readonly EhDbRepository _ehDbRepository;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly EhGlobalValueRepository _ehGlobalValueRepository;

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
                // This would turn SelectedProcItem to null
                ProcItems.Remove(SelectedProcItem);
                await _eventAggregator
                    .PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.OpenDialog,
                        ModernDialog.SelectProcessNoProcessTip)).ConfigureAwait(false);
                return;
            }

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Hide));

            IEnumerable<Process> gameProcesses;
            (gameProcesses, _ehGlobalValueRepository.MainProcess) =
                Utils.ProcessCollect(SelectedProcItem.Proc.ProcessName);
            _ = _gameWindowHooker.SetGameWindowHookAsync();

            var gamePath =
                _ehGlobalValueRepository.GamePath =
                    _ehGlobalValueRepository.MainProcess.MainModule?.FileName ?? string.Empty;
            var md5 = Utils.GetFileMd5(gamePath);
            _ehGlobalValueRepository.Md5 = md5;

            var settingJson = string.Empty;
            var gameInfo = await _ehDbRepository.GetGameInfoAsync(md5).ConfigureAwait(false);
            if (gameInfo is not null)
            {
                settingJson = gameInfo.TextractorSettingJson;
            }
            else
            {
                try
                {
                    using var resp = await _ehServerApiService.GetGameSetting(md5).ConfigureAwait(true);
                    // For throw ApiException
                    //resp = await resp.EnsureSuccessStatusCodeAsync();
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var content = resp.Content ?? new GameSettingResponse();
                        settingJson = content.GameSettingJson;
                    }
                    Log.Debug($"EHServer: {resp.StatusCode} {resp.Content}");
                }
                catch (HttpRequestException ex)
                {
                    Log.Warn("Can't connect to internet", ex);
                }
            }

            if (settingJson == string.Empty)
            {
                Log.Info("Not find game hook setting, open hook panel.");
                await _windowManager.ShowWindowFromIoCAsync<HookConfigViewModel>().ConfigureAwait(false);
                _textractorService.InjectProcesses(gameProcesses);
                _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
                return;
            }

            var textractorSetting = JsonSerializer.Deserialize<TextractorSetting>(settingJson) ?? new TextractorSetting();
            _textractorService.InjectProcesses(gameProcesses, textractorSetting);

            // NOTE: WindowManger每次都会创建新窗口，之后再调相同的VM的话就会创建新的窗口，所以必须通过VM或Message来操作相应View
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

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