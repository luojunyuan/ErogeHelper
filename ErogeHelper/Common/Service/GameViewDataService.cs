using Caliburn.Micro;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.ViewModels;
using ErogeHelper.ViewModels.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ErogeHelper.Common.Service
{
    class GameViewDataService : IGameViewDataService
    {
        public event IGameViewDataService.SourceDataEventHandler? SourceDataEvent;
        public event IGameViewDataService.AppendDataEventHandler? AppendDataEvent;

        private readonly MecabHelper mecabHelper = new MecabHelper();
        public static TextTemplateType SourceTextTemplate = DataRepository.TextTemplateConfig;

        public void Start()
        {
            Textractor.SelectedDataEvent += DataProgress;
        }

        private void DataProgress(object sender, HookParam hp)
        {
            // Refresh
            IoC.Get<GameViewModel>().AppendTextList.Clear(); // Clear or give new value? is that same

            // Regexp
            //var pattern = SimpleIoc.Default.GetInstance<GameInfo>().Regexp;
            //if (!string.IsNullOrEmpty(pattern))
            //{
            //    var list = Regex.Split(hp.Text, pattern);
            //    hp.Text = string.Join("", list);
            //}

            // Todo 4: Add some notify to user, make The max lenth user define able
            //if (hp.Text.Length > 80)
            //{
            //    return;
            //}

            // DeepL Extension
            if (DataRepository.PasteToDeepL)
            {
                Process[] temp = Process.GetProcessesByName("DeepL");
                if (temp.Length != 0)
                {
                    IntPtr handle = temp[0].MainWindowHandle;
                    NativeMethods.SwitchToThisWindow(handle);
                    // TODO 6: check the front window is handle, else toast user得注意这些操作时间开销
                    // Do SetText and Paste both
                    new SetClipboardHelper(DataFormats.Text, hp.Text).Go(); 
                }
            }

            if (DataRepository.MecabEnable)
            {
                var collect = new BindableCollection<SingleTextItem>();

                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    // DependencySource on same Thread as the DependencyObject

                    // Todo 5:
                    // if (DataRepository.MecabEnable) or X TextTemplateType.Default or kana none(same as default)
                    var mecabWordList = mecabHelper.SentenceHandle(hp.Text);
                    foreach (MecabWordInfo mecabWord in mecabWordList)
                    {
                        collect.Add(new SingleTextItem
                        {
                            Text = mecabWord.Word,
                            RubyText = mecabWord.Kana,
                            PartOfSpeed = mecabWord.PartOfSpeech,
                            TextTemplateType = SourceTextTemplate
                        });
                    }
                });

                SourceDataEvent?.Invoke(typeof(GameViewDataService), collect);
            }

            //string result = SakuraNoUtaHelper.QueryText(hp.Text.Trim());
            //if (!string.IsNullOrWhiteSpace(result))
            //    AppendDataEvent?.Invoke(typeof(GameViewDataService), result);
        }

        private void ClipboardPasteSTA(string text)
        {
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join(); //Wait for the thread to end
        }
    }
}
