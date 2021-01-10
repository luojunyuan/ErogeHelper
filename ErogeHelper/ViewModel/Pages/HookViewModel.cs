using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ErogeHelper.ViewModel.Pages
{
    class HookViewModel : PropertyChangedBase
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(HookViewModel));

        #region HookCode
        // InputCode 只在值有效时xaml才会传值过来
        public string InputCode { get; set; } = string.Empty;

        public bool InvalidHookCode { get; set; }

        public bool CanInsertCode() => !string.IsNullOrWhiteSpace(InputCode) && !InvalidHookCode;

        public void InsertCode(object inputCode) => Textractor.InsertHook(InputCode);
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
        private IHookSettingPageService dataService;
        private IWindowManager windowManager;

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
                // When this setter happend, SelectedHook suddenly not null
                selectedTextHandle = SelectedHook!.Handle;
                SelectedText = Utils.TextEvaluateWithRegExp(SelectedHook.Text, RegExp ?? string.Empty);
                NotifyOfPropertyChange(() => CanSubmitSetting);
            }
        }

        private void DataProcess(object sender, HookParam hp)
        {
            if (hp.Name == "控制台") // it means console
            {
                // https://github.com/lgztx96/texthost/blob/master/texthost/texthost.cpp
                if (Language.Strings.Culture.Name != "zh-Hans")
                {
                    hp.Text = hp.Text switch
                    {
                        "Textractor: 已经注入" => "Textractor: already injected",
                        "Textractor: 无效特殊码" => "Textractor: invalid code",
                        "Textractor: 初始化完成" => "Textractor: initialization completed",
                        "Textractor: 无法注入" => "Textractor: couldn't inject",
                        _ => hp.Text
                    };
                }
                ConsoleOutput += "\n" + hp.Text;
                return;
            }
            else if (hp.Name == "剪贴板") // Clipboard
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
                    var count = tmp.Count(f => f == '\n');
                    if (count > 5)
                    {
                        var index = tmp.IndexOf('\n') + 2;
                        index = tmp.IndexOf('\n', index);
                        tmp = tmp.Substring(index);
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

        public bool CanSubmitSetting { get => SelectedHook is not null && !InvalidRegExp; }
        public async void SubmitSetting()
        {
            var configPath = DataRepository.MainProcess!.MainModule!.FileName + ".eh.config";

            GameConfig.IsUserHook = SelectedHook!.EngineName.Contains("UserHook");
            GameConfig.HookCode = SelectedHook.HookCode;
            GameConfig.ThreadContext = SelectedHook.ThreadContext;
            GameConfig.SubThreadContext = SelectedHook.SubThreadContext;
            GameConfig.RegExp = RegExp ?? string.Empty;

            if (File.Exists(configPath))
            {
                // Cover override
                GameConfig.CreateConfig(configPath);
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
        private string totalText = string.Empty;
        private string text = string.Empty;

        public long Handle { get; set; }

        public string Text { get => text; set { text = value; NotifyOfPropertyChange(() => Text); } }

        public string TotalText
        {
            get => totalText;
            set
            {
                totalText = value;
                NotifyOfPropertyChange(() => TotalText);
            }
        }

        public string HookCode { get; set; } = string.Empty;

        public string EngineName { get; set; } = string.Empty;

        public long ThreadContext { get; set; }

        public long SubThreadContext { get; set; }
    }
}
