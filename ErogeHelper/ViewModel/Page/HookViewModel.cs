using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Entity.Table;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace ErogeHelper.ViewModel.Page
{
    public class HookViewModel : PropertyChangedBase
    {
        public HookViewModel(
            IHookDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            EhDbRepository ehDbRepository,
            EhConfigRepository ehConfigRepository,
            EhGlobalValueRepository ehGlobalValueRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _ehDbRepository = ehDbRepository;
            _ehConfigRepository = ehConfigRepository;
            _ehGlobalValueRepository = ehGlobalValueRepository;

            _textractorService.DataEvent += DataProcess;
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }

        private readonly IHookDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly EhDbRepository _ehDbRepository;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly EhGlobalValueRepository _ehGlobalValueRepository;

        #region RegExp

        private string? _regExp = string.Empty;

        // if user click 'x', it will turn to null
        public string? RegExp
        {
            get => _regExp ?? string.Empty;
            set
            {
                _regExp = value;
                if (value is null)
                {
                    SelectedText = SelectedHook?.Text ?? string.Empty;
                }
                else if (!InvalidRegExp)
                {
                    SelectedText = Utils.TextEvaluateWithRegExp(SelectedHook?.Text ?? string.Empty, value);
                }
                NotifyOfPropertyChange(() => RegExp);
            }
        }

        private bool _invalidRegExp;

        // RegExp 无效的状态
        public bool InvalidRegExp
        { get => _invalidRegExp; set { _invalidRegExp = value; NotifyOfPropertyChange(() => CanSubmitSetting); } }

        private string _selectedText = string.Empty;
        public string SelectedText
        { get => _selectedText; set { _selectedText = value; NotifyOfPropertyChange(() => SelectedText); } }

        private const string Tag1 = ".*(?=[「|『])";
        private const string Tag2 = "(?<=[」|』]).*";
        private const string Tag3 = "<.*?>";
        private const string Tag4 = "_r|<br>|#n|\\n";
        private const string Tag5 = "[\\x00-\\xFF]";

        public void RegExpTag1() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag1 : $"{RegExp}|{Tag1}";
        public void RegExpTag2() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag2 : $"{RegExp}|{Tag2}";
        public void RegExpTag3() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag3 : $"{RegExp}|{Tag3}";
        public void RegExpTag4() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag4 : $"{RegExp}|{Tag4}";
        public void RegExpTag5() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag5 : $"{RegExp}|{Tag5}";

        #endregion

        #region Console

        private string _consoleOutput = string.Empty;
        public string ConsoleOutput
        { get => _consoleOutput; set { _consoleOutput = value; NotifyOfPropertyChange(() => ConsoleOutput); } }

        #endregion

        #region HookCode ReadCode

        // Fields
        private bool _canOpenDialog = true;
        private string _inputHCode = string.Empty;
        private bool _canSearchCode = true;

        public bool CanOpenDialog
        { get => _canOpenDialog; set { _canOpenDialog = value; NotifyOfPropertyChange(() => CanOpenDialog); } }

        public async void OpenHCodeDialog()
        {
            CanOpenDialog = false;
            // 如果是new Dialog，窗口在关闭后立即被释放
            // 但如果持有一个或多个实例，必须先等他们 Close 再 Open
            // 否则会引发 "Only a single ContentDialog can be open at any time."
            // “指定的 Visual 已经是另一个 Visual 的子级或者已经是 CompositionTarget 的根。”
            await _eventAggregator.PublishOnUIThreadAsync(
                new ViewActionMessage(GetType(), ViewAction.OpenDialog, ModernDialog.HookCode, null, ViewType.Page))
                .ConfigureAwait(false);
            CanOpenDialog = true;
        }

        public string InputHCode
        {
            get => _inputHCode;
            set
            {
                // 因 validation 约束 InputHCode 只在值有效、或null empty时，xaml才会传值过来
                _inputHCode = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                NotifyOfPropertyChange(() => InputHCode);
            }
        }


        // HCodeDialog Search Button
        public bool CanSearchCode
        { get => _canSearchCode; set { _canSearchCode = value; NotifyOfPropertyChange(() => CanSearchCode); } }

        public async void SearchHCode()
        {
            Log.Debug(InputHCode);
            CanSearchCode = false;
            var hcode = await _dataService.QueryHCode(_ehGlobalValueRepository.Md5).ConfigureAwait(false);
            if (hcode != string.Empty)
            {
                InputHCode = hcode;
                Log.Info(hcode);
            }
            else
            {
                InputHCode = Language.Strings.HookPage_CodeSearchNoResult;
            }
            CanSearchCode = true;
        }

        public void InsertCode()
        {
            // Condition here cause "Enter" key will cross this
            if (InputHCode != string.Empty)
            {
                _textractorService.InsertHook(InputHCode);
            }
            //Log.Debug("Enter key active here!");
        }

        #endregion

        public void ClearHookMapData()
        {
            SelectedHook = null;
            HookMapData.Clear();
        }

        public HookBindingList<long, HookMapItem> HookMapData { get; set; } = new(p => p.Handle);

        private long _selectedTextHandle;

        private HookMapItem? _selectedHook;

        public HookMapItem? SelectedHook
        {
            get => _selectedHook;
            set
            {
                _selectedHook = value;

                if (value is not null)
                {
                    _selectedTextHandle = value.Handle;
                    SelectedText = Utils.TextEvaluateWithRegExp(value.Text, RegExp ?? string.Empty);
                }
                else
                {
                    _selectedTextHandle = -1;
                    SelectedText = string.Empty;
                }
                NotifyOfPropertyChange(() => CanSubmitSetting);
            }
        }

        private async void DataProcess(HookParam hp)
        {
            switch (hp.Handle)
            {
                // Console
                case 0:
                    // https://github.com/lgztx96/texthost/blob/master/texthost/texthost.cpp
                    hp.Text = hp.Text switch
                    {
                        "Textractor: already injected" => Language.Strings.Textractor_AlreadyInject,
                        "Textractor: invalid code" => Language.Strings.Textractor_InvalidCode,
                        "Textractor: initialization completed" => Language.Strings.Textractor_Init,
                        "Textractor: couldn't inject" => Language.Strings.Textractor_InjectFailed,
                        "Textractor: invalid process" => Language.Strings.Textractor_InvalidProcess,
                        _ => hp.Text
                    };
                    ConsoleOutput += "\n" + hp.Text;
                    return;
                // Clipboard
                case 1:
                    hp.Name = Language.Strings.Textractor_Clipboard;
                    return;
            }

            // Note: this await may cause problem
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Error Info: 在“ItemAdded”事件后具有意外的长度。\n
                // 如果在没有引发相应 ListChanged 事件的情况下更改了 IBindingList，则会出现这种情况。
                var targetItem = HookMapData.FastFind(hp.Handle);
                if (targetItem == null)
                {
                    targetItem = new HookMapItem
                    {
                        Handle = hp.Handle,
                        HookCode = hp.Hookcode,
                        ThreadContext = hp.Ctx,
                        SubThreadContext = hp.Ctx2,
                        TotalText = hp.Text,
                        Text = hp.Text,
                        EngineName = hp.Name
                    };
                    HookMapData.Insert(0, targetItem);
                }
                else
                {
                    string tmp = targetItem.TotalText + "\n\n" + hp.Text;

                    // HACK: Dummy way to generate my TextBlock item text
                    var count = tmp.Count(f => f.Equals('\n'));
                    if (count > 5)
                    {
                        var index = tmp.IndexOf('\n') + 2;
                        index = tmp.IndexOf('\n', index);
                        tmp = tmp[index..];
                    }
                    targetItem.TotalText = tmp;
                    targetItem.Text = hp.Text;
                }
            });

            if (SelectedHook is null && _ehGlobalValueRepository.TextractorSetting.Hookcode != string.Empty)
            {
                var setting = _ehGlobalValueRepository.TextractorSetting;
                if (setting.Hookcode.Equals(hp.Hookcode)
                    && (setting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                    && setting.SubThreadContext == hp.Ctx2)
                {
                    // UNDONE: Pending to test
                    SelectedHook = HookMapData.FastFind(hp.Handle);
                }
            }

            if (_selectedTextHandle == hp.Handle)
            {
                SelectedText = Utils.TextEvaluateWithRegExp(hp.Text, RegExp ?? string.Empty);
            }
        }

        public bool CanSubmitSetting => SelectedHook is not null && !InvalidRegExp;

        public async void SubmitSetting()
        {
            var textPendingToSend = SelectedText
                .Replace("|~S~|", string.Empty)
                .Replace("|~E~|", string.Empty);

            if (!string.IsNullOrWhiteSpace(RegExp))
            {
                var list = Regex.Split(textPendingToSend, RegExp);
                textPendingToSend = string.Join("", list);
            }

            // UNDONE: Invoke sendText to GameView, pass through MeCab and color

            if (SelectedHook is null)
                throw new ArgumentNullException(nameof(SelectedHook));

            _ehGlobalValueRepository.TextractorSetting.IsUserHook = SelectedHook.EngineName.Contains("UserHook");
            _ehGlobalValueRepository.TextractorSetting.Hookcode = SelectedHook.HookCode;
            _ehGlobalValueRepository.TextractorSetting.ThreadContext = SelectedHook.ThreadContext;
            _ehGlobalValueRepository.TextractorSetting.SubThreadContext = SelectedHook.SubThreadContext;
            _ehGlobalValueRepository.TextractorSetting.RegExp = RegExp ?? string.Empty;

            // 用一个开关? 异步
            // UNDONE: ehApi SubmitSetting with gameNames RCode不要

            var gameInfoTable =
                await _ehDbRepository.GetGameInfoAsync(_ehGlobalValueRepository.Md5).ConfigureAwait(false);
            if (gameInfoTable is null)
            {
                // HookPage must in HookConfigView window
                // This happen when server has no data or user uses EH offline
                gameInfoTable = new GameInfoTable
                {
                    Md5 = _ehGlobalValueRepository.Md5,
                    GameIdList = string.Empty,
                    GameSettingJson = JsonSerializer.Serialize(_ehGlobalValueRepository.TextractorSetting),
                };
                await _ehDbRepository.SetGameInfoAsync(gameInfoTable).ConfigureAwait(false);
            }
            else
            {
                var hookSetting = gameInfoTable.GameSettingJson;
                if (hookSetting == string.Empty)
                {
                    // HookPage in HookConfigView window
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        GameSettingJson = JsonSerializer.Serialize(_ehGlobalValueRepository.TextractorSetting),
                    }).ConfigureAwait(false);
                }
                else
                {
                    // HookPage in Preference window
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        GameSettingJson = JsonSerializer.Serialize(_ehGlobalValueRepository.TextractorSetting),
                    }).ConfigureAwait(false);
                    Log.Debug("Update db.sqlite");
                    // QUESTION: 也许我不用在Page中OpenDialog，而在Window中。可能都一样
                    await _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(),
                            ViewAction.OpenDialog, ModernDialog.HookSettingUpdatedTip, null, ViewType.Page))
                        .ConfigureAwait(false);
                    return;
                }
            }

            // 在HookViewModel(page vm)以HookConfigViewModel(window vm)的名义向HookConfigView(window)发消息
            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Hide));

            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            _ = _ehConfigRepository.UseOutsideWindow
                ? _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"))
                : _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Close));
        }

#pragma warning disable 8618
        public HookViewModel() { }
    }
}