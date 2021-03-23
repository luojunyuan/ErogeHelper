using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using System;
using System.Globalization;

namespace ErogeHelper.ViewModel.Window
{
    public class SelectProcessViewModel : PropertyChangedBase, IDisposable
    {
        public SelectProcessViewModel(
            ISelectProcessDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            IGameWindowHooker gameWindowHooker,
            EhConfigRepository ehConfigRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _gameWindowHooker = gameWindowHooker;
            _ehConfigRepository = ehConfigRepository;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly IGameWindowHooker _gameWindowHooker;
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
                // XXX: 用户此时来个快速操作，ProcItems.Remove(null)，无事发生
                ProcItems.Remove(SelectedProcItem);
                return;
            }

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Hide));

            (_ehConfigRepository.GameProcesses, _ehConfigRepository.MainProcess) = 
                Utils.ProcessCollect(SelectedProcItem.Proc.ProcessName);

            // XXX: 注意WindowManger每次都会创建新窗口，之后再调相同的VM的话就会创建新的窗口，所以必须通过VM或Message来操作相应View
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView");
            _ = _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView");

            // gameWindowHooker 在设置是立马就会发送游戏当前坐标，就开始启用了，所以必须先准备InsideView的，event，Outside不受影响
            _ = _gameWindowHooker.SetGameWindowHookAsync();

            // 在调textractor之前先检查本地textsetting
            // 没有 再拿md5从服务器获取id textsetting 所以这个过程可以和gameHooker一起，异步进行，gamehooker可以异步吗
            _ehConfigRepository.TextractorSetting = new HookParam();
            _textractorService.InjectProcesses();

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
        }

        public void Dispose() => Log.Debug($"{nameof(SelectProcessViewModel)}.Dispose()");

#pragma warning disable 8618
        public SelectProcessViewModel() { }
    }
}