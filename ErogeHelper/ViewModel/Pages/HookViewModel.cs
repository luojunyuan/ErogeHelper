using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.Model.Api;
using ModernWpf.Controls;
using Serilog;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ErogeHelper.ViewModel.Pages
{
    class HookViewModel : PropertyChangedBase
    {
        #region HookCode
        // InputCode 只在值有效时xaml才会传值过来
        private string _inputCode = string.Empty;
        public string InputCode
        {
            get => _inputCode;
            set
            {
                _inputCode = value;
                NotifyOfPropertyChange(() => InputCode);
                NotifyOfPropertyChange(() => CanInsertCode);
            }
        }
        public bool InvalidHookCode { get; set; }

        private bool _canSearchCode = true;
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
            var hcode = await QueryHCode.QueryCode(GameConfig.MD5).ConfigureAwait(false);
            if (!hcode.Equals(string.Empty))
            {
                InputCode = hcode;
                Log.Info(hcode);
            }
            else
            {
                InputCode = "No Result";
            }
            CanSearchCode = true;
        }

        public bool CanInsertCode => !string.IsNullOrWhiteSpace(InputCode) && !InvalidHookCode;
        public void InsertCode()
        {
            if (CanInsertCode)
            {
                Textractor.InsertHook(InputCode);
            }
        }
        public void DialogClosingEvent(ContentDialogClosingEventArgs args)
        {
            // Block Enter key and PrimaryButton
            if (args.Result == ContentDialogResult.Primary && !CanInsertCode)
            {
                args.Cancel = true;
            }
            // Only let CloseButton and Escape key go
            //if (args.Result == ContentDialogResult.None)
        }
        #endregion

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
                else if (!InvalidRegExp && value is not null)
                {
                    SelectedText = Utils.TextEvaluateWithRegExp(SelectedHook?.Text ?? string.Empty, value);
                }
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

        private long selectedTextHandle;
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
        #endregion

        private string _consoleOutput = string.Empty;
        public string ConsoleOutput
        {
            get => _consoleOutput;
            set
            {
                _consoleOutput = value;
                NotifyOfPropertyChange(() => ConsoleOutput);
            }
        }

        #region Constructor
        private readonly IHookSettingPageService dataService;
        private readonly IWindowManager windowManager;

        public HookViewModel(IHookSettingPageService dataService, IWindowManager windowManager)
        {
            this.dataService = dataService;
            this.windowManager = windowManager;

            RegExp = dataService.GetRegExp();
            Textractor.DataEvent += DataProcess;
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }
        #endregion

        public HookBindingList<long, HookMapItem> HookMapData { get; set; } = new HookBindingList<long, HookMapItem>(p => p.Handle);

        private HookMapItem? _selectedHook;

        public HookMapItem? SelectedHook
        {
            get => _selectedHook;
            set
            {
                _selectedHook = value;

                if (value is not null)
                {
                    selectedTextHandle = value.Handle;
                    SelectedText = Utils.TextEvaluateWithRegExp(value.Text, RegExp ?? string.Empty);
                }
                else
                {
                    selectedTextHandle = -1;
                    SelectedText = string.Empty;
                }
                NotifyOfPropertyChange(() => CanSubmitSetting);
            }
        }

        public void ClearHookMapData()
        {
            SelectedHook = null;
            HookMapData.Clear();
        }

        private void DataProcess(object sender, HookParam hp)
        {
            if (hp.Handle == 0) // Console
            {
                // https://github.com/lgztx96/texthost/blob/master/texthost/texthost.cpp
                if (!Language.Strings.Culture.Name.Equals("zh-Hans"))
                {
                    hp.Text = hp.Text switch
                    {
                        "Textractor: 已经注入" => "Textractor: already injected",
                        "Textractor: 无效特殊码" => "Textractor: invalid code",
                        "Textractor: 初始化完成" => "Textractor: initialization completed",
                        "Textractor: 无法注入" => "Textractor: couldn't inject",
                        "Textractor: 无效进程ID" => "Textractor: invalid process",
                        _ => hp.Text
                    };
                }
                ConsoleOutput += "\n" + hp.Text;
                return;
            }
            else if (hp.Handle == 1) // Clipboard
            {
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

                    // dummy way to do my TextBlock item
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

            if (selectedTextHandle == hp.Handle)
            {
                SelectedText = Utils.TextEvaluateWithRegExp(hp.Text, RegExp ?? string.Empty);
            }
        }

        public bool CanSubmitSetting => SelectedHook is not null && !InvalidRegExp;
        public async void SubmitSetting()
        {
            var configPath = DataRepository.MainProcess!.MainModule!.FileName + ".eh.config";

            GameConfig.IsUserHook = SelectedHook!.EngineName.Contains("UserHook");
            GameConfig.HookCode = SelectedHook.HookCode;
            GameConfig.ThreadContext = SelectedHook.ThreadContext;
            GameConfig.SubThreadContext = SelectedHook.SubThreadContext;
            GameConfig.RegExp = RegExp ?? string.Empty;

            // TODO: Refactor
            var sendText = SelectedText;
            if (!string.IsNullOrWhiteSpace(RegExp))
            {
                var list = Regex.Split(sendText, RegExp);
                sendText = string.Join("", list);
            }
            IoC.Get<GameViewModel>().dataService.RefreshCurentMecabText(sendText);

            if (File.Exists(configPath))
            {
                // Cover override
                GameConfig.CreateConfig(configPath);
                await new ContentDialog
                {
                    Content = $"Update {DataRepository.MainProcess.ProcessName}.exe.eh.config succeed",
                    CloseButtonText = "OK"
                }.ShowAsync().ConfigureAwait(false);
            }
            else
            {
                GameConfig.CreateConfig(configPath);
                await windowManager.ShowWindowAsync(IoC.Get<GameViewModel>()).ConfigureAwait(false);
                await IoC.Get<HookConfigViewModel>().TryClose().ConfigureAwait(false);
            }
        }
    }

    class HookMapItem : PropertyChangedBase
    {
        private string _totalText = string.Empty;
        private string _text = string.Empty;

        public long Handle { get; set; }

        public string Text { get => _text; set { _text = value; NotifyOfPropertyChange(() => Text); } }

        public string TotalText
        {
            get => _totalText;
            set
            {
                _totalText = value;
                NotifyOfPropertyChange(() => TotalText);
            }
        }

        public string HookCode { get; set; } = string.Empty;

        public string EngineName { get; set; } = string.Empty;

        public long ThreadContext { get; set; }

        public long SubThreadContext { get; set; }
    }
}
