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

        // InputCode 只在值有效时xaml才会传值过来
        public string InputCode { get; set; } = string.Empty;

        public bool HasError { get; set; }

        public bool CanInsertCode() => !string.IsNullOrWhiteSpace(InputCode) && !HasError;

        public void InsertCode(object inputCode) => Textractor.InsertHook(InputCode);

        public string RegExp { get; set; }

        private string consoleOutput = string.Empty;
        public string ConsoleOutput { get => consoleOutput; set { consoleOutput = value; NotifyOfPropertyChange(() => ConsoleOutput); } }

        private IHookSettingPageService dataService;
        private IWindowManager windowManager;

        public HookViewModel(IHookSettingPageService dataService, IWindowManager windowManager)
        {
            this.dataService = dataService;
            this.windowManager = windowManager;

            RegExp = dataService.GetRegexp();
            Textractor.DataEvent += DataProcess;
        }
        public HookBindingList<long, HookMapItem> HookMapData { get; set; } = new HookBindingList<long, HookMapItem>(p => p.Handle);

        public HookMapItem SelectedHook { set; get; } = new HookMapItem();

        private void DataProcess(object sender, HookParam hp)
        {
            if (hp.Name == "控制台") // it means console
            {
                // 初始化完成
                // 无效特殊码
                ConsoleOutput += "\n" + hp.Text;
                return;
            }
            // Had turn off textractor clipboard monitor
            //else if (hp.Name == "剪贴板")
            //{
            //    // ClipboardOutput += "\n" + hp.Text;
            //    return;
            //}

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
                        ThreadContext= hp.Ctx,
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

                    // dummy way with my TextBlock item
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
        }

        public bool CanSubmitSetting() => true;
        public async void SubmitSetting(object _, object __)
        {
            var configPath = DataRepository.MainProcess!.MainModule!.FileName + ".eh.config";

            GameConfig.IsUserHook = SelectedHook.EngineName.Contains("UserHook") ? true : false;
            GameConfig.HookCode = SelectedHook.HookCode;
            GameConfig.ThreadContext = SelectedHook.ThreadContext;
            GameConfig.SubThreadContext = SelectedHook.SubThreadContext;
            GameConfig.Regexp = RegExp;

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
