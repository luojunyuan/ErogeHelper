using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ModernWpf.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ErogeHelper.ViewModel.Page
{
    public class HookViewModel : PropertyChangedBase
    {
        public HookViewModel(
            IHookDataService dataService,
            IWindowManager windowManager,
            ITextractorService textractorService,
            EhConfigRepository ehConfigRepository,
            EhGlobalValueRepository ehGlobalValueRepository)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _textractorService = textractorService;
            _ehConfigRepository = ehConfigRepository;
            _ehGlobalValueRepository = ehGlobalValueRepository;

            //RegExp = dataService.GetRegExp();
            _textractorService.DataEvent += DataProcess;
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }

        private readonly IHookDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly ITextractorService _textractorService;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly EhGlobalValueRepository _ehGlobalValueRepository;

        #region RegExp
        private string? _regExp;
        public string? RegExp // if user click 'x', it will turn to null
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
        public bool InvalidRegExp // trans: 无效的RegExp
        {
            get => _invalidRegExp;
            set
            {
                _invalidRegExp = value;
                NotifyOfPropertyChange(() => CanSubmitSetting);
            }
        }

        private long _selectedTextHandle;
        private string _selectedText = string.Empty;
        public string SelectedText
        {
            get => _selectedText;
            set
            {
                _selectedText = value;
                NotifyOfPropertyChange(() => SelectedText);
            }
        }

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

        #region HookCode

        private string _inputCode = string.Empty;
        private bool _canSearchCode = true;

        public string InputCode
        {
            get => _inputCode;
            set
            {
                // InputCode 只在值有效时xaml才会传值过来
                _inputCode = value;
                NotifyOfPropertyChange(() => InputCode);
                NotifyOfPropertyChange(() => CanInsertCode);
            }
        }

        public bool InvalidHookCode { get; set; }

        public bool CanSearchCode
        {
            get => _canSearchCode;
            set
            {
                _canSearchCode = value;
                NotifyOfPropertyChange(() => CanSearchCode);
            }
        }
        public async void SearchCode()
        {
            CanSearchCode = false;
            var hcode = await _dataService.QueryHCode(_ehGlobalValueRepository.Md5).ConfigureAwait(false);
            if (hcode != string.Empty)
            {
                InputCode = hcode;
                Log.Info(hcode);
            }
            else
            {
                InputCode = Language.Strings.HookPage_CodeSearchNoResult;
            }
            CanSearchCode = true;
        }

        public bool CanInsertCode => !string.IsNullOrWhiteSpace(InputCode) && !InvalidHookCode;
        public void InsertCode()
        {
            if (CanInsertCode)
            {
                _textractorService.InsertHook(InputCode);
            }
        }
        public void DialogClosingEvent(ContentDialogClosingEventArgs args)
        {
            // TODO
            // Block Enter key and PrimaryButton
            if (args.Result == ContentDialogResult.Primary && !CanInsertCode)
            {
                args.Cancel = true;
            }
            // Only let CloseButton and Escape key go
            //if (args.Result == ContentDialogResult.None)
        }
        #endregion

        public void ClearHookMapData()
        {
            SelectedHook = null;
            HookMapData.Clear();
        }

        public HookBindingList<long, HookMapItem> HookMapData { get; set; } = new(p => p.Handle);

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

        private void DataProcess(HookParam hp)
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

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Error Info: 在“ItemAdded”事件后具有意外的长度。\n如果在没有引发相应 ListChanged 事件的情况下更改了 IBindingList，则会出现这种情况。
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

                    // HACK: Dummy way to do my TextBlock item
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

            if (_selectedTextHandle == hp.Handle)
            {
                SelectedText = Utils.TextEvaluateWithRegExp(hp.Text, RegExp ?? string.Empty);
            }
        }

        public bool CanSubmitSetting => SelectedHook is not null && !InvalidRegExp;

        public async void SubmitSetting()
        {
            // TODO: Refactor
            var sendText = SelectedText
                .Replace("|~S~|", string.Empty)
                .Replace("|~E~|", string.Empty);

            if (!string.IsNullOrWhiteSpace(RegExp))
            {
                var list = Regex.Split(sendText, RegExp);
                sendText = string.Join("", list);
            }
        }

        #pragma warning disable 8618
        public HookViewModel() { }
    }
}