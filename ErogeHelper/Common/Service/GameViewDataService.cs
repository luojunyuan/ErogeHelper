using Caliburn.Micro;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Control;
using ErogeHelper.ViewModel.Pages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ErogeHelper.Common.Service
{
    class GameViewDataService : IGameViewDataService
    {
        public event IGameViewDataService.SourceDataEventHandler? SourceDataEvent;
        public event IGameViewDataService.AppendDataEventHandler? AppendDataEvent;

        private readonly MecabHelper mecabHelper = new MecabHelper();
        private readonly SakuraNoUtaHelper sakuraNoUtaHelper = new SakuraNoUtaHelper();

        public void Start()
        {
            Textractor.SelectedDataEvent += DataProgress;
        }

        private void DataProgress(object sender, HookParam hp)
        {
            // Refresh
            IoC.Get<GameViewModel>().AppendTextList.Clear(); // Clear or give new value? is that same

            // User define RegExp 
            var pattern = GameConfig.RegExp;
            if (!string.IsNullOrEmpty(pattern))
            {
                var list = Regex.Split(hp.Text, pattern);
                hp.Text = string.Join("", list);
            }

            // Clear ascii control characters
            hp.Text = new string(hp.Text.Select(c => c < ' ' ? '_' : c).ToArray()).Replace("_", string.Empty);
            // Linebreak 
            // Full-width space
            hp.Text = hp.Text.Replace("　", string.Empty);
            // Ruby
            // <.*?>

            if (hp.Text.Length > 120)
            {
                IoC.Get<TextViewModel>().SourceTextCollection.Clear();
                AppendDataEvent?.Invoke(typeof(GameViewDataService), Language.Strings.GameView_MaxLenthTip);
                return;
            }

            // DeepL Extension
            if (DataRepository.PasteToDeepL)
            {
                Process[] temp = Process.GetProcessesByName("DeepL");
                if (temp.Length != 0)
                {
                    IntPtr handle = temp[0].MainWindowHandle;
                    NativeMethods.SwitchToThisWindow(handle);

                    // Do SetText and Paste both
                    new DeepLHelper(DataFormats.Text, hp.Text).Go();

                    if (NativeMethods.GetForegroundWindow() != handle)
                    {
                        // Better use Toast in win10
                        Application.Current.Dispatcher.InvokeAsync(() => ModernWpf.MessageBox.Show(
                            "Didn't find DeepL client in front, will turn off DeepL extension..", "Eroge Helper"));

                        DataRepository.PasteToDeepL = false;
                    }
                }
            }

            // Process source japanese text
            if (IoC.Get<GeneralViewModel>().ShowSource)
            {
                var collect = new BindableCollection<SingleTextItem>();

                foreach (MecabWordInfo mecabWord in mecabHelper.MecabWordEnumerable(hp.Text))
                {
                    collect.Add(new SingleTextItem
                    {
                        Text = mecabWord.Word,
                        RubyText = mecabWord.Kana,
                        PartOfSpeed = mecabWord.PartOfSpeech,
                        TextTemplateType = DataRepository.TextTemplateConfig,
                        SubMarkColor = Utils.Hinshi2Color(mecabWord.PartOfSpeech)
                    });
                }

                SourceDataEvent?.Invoke(typeof(GameViewDataService), collect);
            }

            // hard code for sakura no uta
            //if (GameConfig.MD5.Equals("BAB61FB3BD98EF1F1538EE47A8A46A26"))
            //{
            //    string result = sakuraNoUtaHelper.QueryText(hp.Text);
            //    if (!string.IsNullOrWhiteSpace(result))
            //        AppendDataEvent?.Invoke(typeof(GameViewDataService), result);
            //}
        }
    }
}
