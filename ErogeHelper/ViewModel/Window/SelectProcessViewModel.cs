using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ModernWpf.Controls;

namespace ErogeHelper.ViewModel.Window
{
    public class SelectProcessViewModel : Screen, IDisposable
    {
        public SelectProcessViewModel(
            ISelectProcessDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            EhConfigRepository ehConfigRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _ehConfigRepository = ehConfigRepository;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly EhConfigRepository _ehConfigRepository;

        public BindableCollection<ProcComboBoxItem> ProcItems { get; private set; } = new();

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
                await new ContentDialog
                {
                    Title = "Eroge Helper",
                    Content = Language.Strings.SelectProcess_ProcessExit,
                    CloseButtonText = "OK"
                }.ShowAsync().ConfigureAwait(false);
                // Cause this turn SelectedProcItem to null
                ProcItems.Remove(SelectedProcItem); 
                return;
            }

            //if (GetView() is System.Windows.Window view)
            //    view.Hide();
            await _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Hide));

            var processes = Utils.ProcessCollect(SelectedProcItem.Proc.ProcessName);

            // GameHooker注入进程，拿坐标信息事件 textractor 其实也可以动好像没啥先后关系

            // 在调textractor之前先检查本地textsetting
            // 没有 再拿md5从服务器获取id textsetting 所以这个过程可以和gameHooker一起，异步进行，gamehooker可以异步吗
            _ehConfigRepository.TextractorSetting = new();
            _textractorService.InjectProcesses();

            // 都没有开setting
            await _windowManager.ShowWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);
            //await TryCloseAsync();
            await _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
        }

        public void Dispose() => Log.Debug($"{nameof(SelectProcessViewModel)}.Dispose()");

#pragma warning disable 8618
        public SelectProcessViewModel() { }
    }
}