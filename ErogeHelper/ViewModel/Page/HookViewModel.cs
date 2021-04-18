using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using ErogeHelper.ViewModel.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using ErogeHelper.View.Dialog;

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
            IGameDataService gameDataService)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _ehDbRepository = ehDbRepository;
            _ehConfigRepository = ehConfigRepository;
            _gameDataService = gameDataService;

            _textractorService.DataEvent += DataProcess;
            RegExp = _dataService.GetRegExp();
            ConsoleOutput = string.Join('\n', _textractorService.GetConsoleOutputInfo());
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }

        private readonly IHookDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly EhDbRepository _ehDbRepository;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly IGameDataService _gameDataService;

        #region RegExp

        private string? _regExp = string.Empty;

        // If user click 'x', it will turn to null
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

        public bool InvalidRegExp
        {
            get => _invalidRegExp;
            set { _invalidRegExp = value; NotifyOfPropertyChange(() => CanSubmitSetting); }
        }

        private string _selectedText = string.Empty;
        public string SelectedText
        { get => _selectedText; set { _selectedText = value; NotifyOfPropertyChange(() => SelectedText); } }

        private const string Tag1 = ".*(?=[「|『])";
        private const string Tag2 = "(?<=[」|』]).*";
        private const string Tag3 = "<.*?>";
        private const string Tag4 = "_r|<br>|#n|\\n|\\\\n";
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

        private bool _canOpenDialog = true;
        private string _inputHCode = string.Empty;
        private bool _canSearchCode = true;

        public bool CanOpenDialog
        {
            get => _canOpenDialog;
            set { _canOpenDialog = value; NotifyOfPropertyChange(() => CanOpenDialog); }
        }

        public async void OpenHCodeDialog()
        {
            CanOpenDialog = false;
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
                // 因 validation 约束 InputHCode 只在值有效、null或empty时，xaml才会传值过来
                _inputHCode = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                NotifyOfPropertyChange(() => InputHCode);
            }
        }

        public bool CanSearchCode
        {
            get => _canSearchCode;
            set { _canSearchCode = value; NotifyOfPropertyChange(() => CanSearchCode); }
        }

        public async void SearchHCode()
        {
            CanSearchCode = false;
            var hcode = await _dataService.QueryHCode().ConfigureAwait(false);
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
        }

        public async void OpenRCodeDialog()
        {
            CanOpenDialog = false;
            await _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(GetType(), ViewAction.OpenDialog, ModernDialog.ReadCode, null, ViewType.Page))
                .ConfigureAwait(false);
            CanOpenDialog = true;
        }

        public void SearchRCodeText(RCodeDialog dialog)
        {
            // Condition here cause "Enter" key will cross this
            if (dialog.IsPrimaryButtonEnabled)
            {
                _textractorService.SearchRCode(dialog.JapaneseText.Text);
            }
        }

        #endregion

        public void ClearHookMapData()
        {
            SelectedHook = null;
            HookMapData.Clear();
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
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
                NotifyOfPropertyChange(() => SelectedHook);
            }
        }

        private async void DataProcess(HookParam hp)
        {
            if (hp.Handle == 0)
            {
                // Console
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
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var targetItem = HookMapData.FastFind(hp.Handle);
                if (targetItem is null)
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

            if (SelectedHook is null && _textractorService.Setting.Hookcode != string.Empty)
            {
                var setting = _textractorService.Setting.HookSettings.First();
                if (_textractorService.Setting.Hookcode.Equals(hp.Hookcode)
                    && (setting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                    && setting.SubThreadContext == hp.Ctx2)
                {
                    SelectedHook = HookMapData.FastFind(hp.Handle);
                }
            }

            if (_selectedTextHandle == hp.Handle)
            {
                SelectedText = Utils.TextEvaluateWithRegExp(hp.Text, RegExp ?? string.Empty);
            }
        }

        public bool CanSubmitSetting => SelectedHook is not null && !InvalidRegExp && !_submitting;
        private bool _submitting;

        public async void SubmitSetting()
        {
            _submitting = true;
            NotifyOfPropertyChange(() => CanSubmitSetting);

            var textPendingToSend = SelectedText
                .Replace("|~S~|", string.Empty)
                .Replace("|~E~|", string.Empty);

            if (!string.IsNullOrWhiteSpace(RegExp))
            {
                var list = Regex.Split(textPendingToSend, RegExp);
                textPendingToSend = string.Join("", list);
            }

            if (SelectedHook is null)
                throw new ArgumentNullException(nameof(SelectedHook));

            var textractorSetting = new TextractorSetting
            {
                IsUserHook = SelectedHook.EngineName.Contains("UserHook"),
                Hookcode = SelectedHook.HookCode,
                HookSettings = new List<TextractorSetting.HookSetting>()
                {
                    new()
                    {
                        ThreadContext = SelectedHook.ThreadContext,
                        SubThreadContext = SelectedHook.SubThreadContext,
                    }
                }
            };
            _textractorService.Setting = textractorSetting;
            _ = _eventAggregator.PublishOnUIThreadAsync(new RegExpChangedMessage { RegExp = RegExp ?? string.Empty });

            // 用一个开关? 异步
            // TODO: 先完成服务器重建 ehApi SubmitSetting with gameNames 剪切板，hp.Name Search 不要 

            var gameInfoTable =
                await _ehDbRepository.GetGameInfoAsync().ConfigureAwait(false);
            if (gameInfoTable is null)
            {
                // HookPage must in HookConfigView window
                // This happen when server has no data or user uses EH offline
                gameInfoTable = new GameInfoTable
                {
                    Md5 = _ehDbRepository.Md5,
                    RegExp = RegExp ?? string.Empty,
                    TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                };
                await _ehDbRepository.SetGameInfoAsync(gameInfoTable).ConfigureAwait(false);
            }
            else
            {
                var hookSetting = gameInfoTable.TextractorSettingJson;
                if (hookSetting == string.Empty)
                {
                    // HookPage in HookConfigView window
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        RegExp = RegExp ?? string.Empty,
                        TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                        IsLoseFocus = gameInfoTable.IsLoseFocus,
                        IsEnableTouchToMouse = gameInfoTable.IsEnableTouchToMouse,
                    }).ConfigureAwait(false);
                }
                else
                {
                    // HookPage in Preference window
                    _gameDataService.SendNewText(textPendingToSend);
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        RegExp = RegExp ?? string.Empty,
                        TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                        IsLoseFocus = gameInfoTable.IsLoseFocus,
                        IsEnableTouchToMouse = gameInfoTable.IsEnableTouchToMouse,
                    }).ConfigureAwait(false);
                    await _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(),
                            ViewAction.OpenDialog, ModernDialog.HookSettingUpdatedTip, null, ViewType.Page))
                        .ConfigureAwait(false);

                    _submitting = false;
                    NotifyOfPropertyChange(() => CanSubmitSetting);
                    return;
                }
            }

            // 在HookViewModel(page vm)以HookConfigViewModel(window vm)的名义向HookConfigView(window)发消息
            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Hide));

            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            _gameDataService.SendNewText(textPendingToSend);

            _ = _ehConfigRepository.UseOutsideWindow
                ? _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"))
                : _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Close));

            _submitting = false;
            NotifyOfPropertyChange(() => CanSubmitSetting);
        }

#pragma warning disable 8618
        public HookViewModel() { }
    }
}