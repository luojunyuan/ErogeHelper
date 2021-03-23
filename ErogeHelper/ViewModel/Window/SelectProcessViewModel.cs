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

            #region 这一段必须在确定textsetting状态之后 的原因是 关闭hooksetting窗口程序不会结束 除非我手动让hooksetting窗口关闭后结束整个程序
            // 才可以全部显示出来，要关一起关 怎么办呢，1.关hook窗口等于关整个程序. 2.先确定需不需要hook窗口后再启动俩窗口。我选择后者


            // XXX: 注意WindowManger每次都会创建新窗口，之后再调相同的VM的话就会创建新的窗口，所以必须通过VM或Message来操作相应View
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView");
            _ = _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView");

            // gameWindowHooker 在设置是立马就会发送游戏当前坐标，就开始启用了，所以必须先准备InsideView的，event，Outside不受影响
            _ = _gameWindowHooker.SetGameWindowHookAsync(); // 发送了坐标后窗口立马回收到信号，必须加一个开关，或者看看能不能隐藏因为要之后再
            // 决定显示谁？
            #endregion

            // 在调textractor之前先检查本地textsetting
            // 没有 再拿md5从服务器获取id textsetting 所以这个过程可以和gameHooker一起，异步进行，gamehooker可以异步吗
            _ehConfigRepository.TextractorSetting = new HookParam();
            _textractorService.InjectProcesses();

            // 必须确定textsetting的状态后再来决定显示拿个窗口（且）
            _ = _eventAggregator.PublishOnUIThreadAsync(
                new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
        }

        public void Dispose() => Log.Debug($"{nameof(SelectProcessViewModel)}.Dispose()");

#pragma warning disable 8618
        public SelectProcessViewModel() { }
    }
}