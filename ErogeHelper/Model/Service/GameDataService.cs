using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Function;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service
{
    public class GameDataService : IGameDataService
    {
        public event Action<string>? SourceTextReceived;
        public event Action<BindableCollection<SingleTextItem>>? BindableTextItem;
        public event Action<string, string>? AppendTextReceived;
        public event Action<object>? AppendTextsRefresh;

        public GameDataService(
            IMeCabService meCabService,
            ITextractorService textractorService, 
            EhGlobalValueRepository ehGlobalValueRepository,
            EhConfigRepository ehConfigRepository)
        {
            _meCabService = meCabService;
            _ehGlobalValueRepository = ehGlobalValueRepository;
            _ehConfigRepository = ehConfigRepository;
         
            textractorService.SelectedDataEvent += ProcessDataText;
        }

        private readonly IMeCabService _meCabService;
        private readonly EhGlobalValueRepository _ehGlobalValueRepository;
        private readonly EhConfigRepository _ehConfigRepository;

        private void ProcessDataText(HookParam hp)
        {
            // Refresh
            AppendTextsRefresh?.Invoke(nameof(GameDataService));

            // User define RegExp 
            // UNDONE: submit的同时修改到这儿、就不使用运行时变量，直接从repo读？
            var pattern = _ehGlobalValueRepository.TextractorSetting.RegExp;
            if (pattern != string.Empty)
            {
                var list = Regex.Split(hp.Text, pattern);
                hp.Text = string.Join("", list);
            }

            // Clear ascii control characters
            hp.Text = new string(hp.Text.Select(c => c < ' ' ? '_' : c).ToArray()).Replace("_", string.Empty);
            // LineBreak 
            // Full-width space
            hp.Text = hp.Text.Replace("　", string.Empty);
            // Ruby like <.*?>

            if (hp.Text.Length > 120)
            {
                BindableTextItem?.Invoke(new BindableCollection<SingleTextItem>());
                AppendTextReceived?.Invoke(Language.Strings.GameView_MaxLenthTip, string.Empty);
                return;
            }

            SourceTextReceived?.Invoke(hp.Text);

            // DeepL Extension
            if (_ehConfigRepository.PasteToDeepL)
                SendTextToDeepL(hp.Text);

            // Process source japanese text
            if (_ehConfigRepository.EnableMeCab)
            {
                BindableTextItem?.Invoke(Utils.BindableTextMaker(
                    hp.Text,
                    _meCabService.MeCabWordUniDicEnumerable,
                    _ehConfigRepository.TextTemplateConfig));
            }

            // Process translations
            //foreach (var translator in TranslatorManager.GetEnabled())
            //{
            //    Task.Run(async () =>
            //    {
            //        Stopwatch sw = new();
            //        sw.Start();
            //        var result = await translator.TranslateAsync(hp.Text, DataRepository.TransSrcLanguage, DataRepository.TransTargetLanguage);
            //        sw.Stop();
            //        if (!result.Equals(string.Empty))
            //        {
            //            Log.Debug($"{translator.Name}: {result}");
            //            AppendDataEvent?.Invoke(typeof(GameViewDataService), result, $"{translator.Name} {sw.ElapsedMilliseconds}ms");
            //        }
            //    });
            //}

            // hard code for sakura no uta
            //if (GameConfig.MD5.Equals("BAB61FB3BD98EF1F1538EE47A8A46A26"))
            //{
            //    string result = sakuraNoUtaHelper.QueryText(hp.Text);
            //    if (!string.IsNullOrWhiteSpace(result))
            //        AppendDataEvent?.Invoke(typeof(GameViewDataService), result, "人工字幕 by luki");
            //}
        }

        private void SendTextToDeepL(string text)
        {
            Process[] temp = Process.GetProcessesByName("DeepL");
            if (temp.Length == 0)
                return;

            var handle = temp[0].MainWindowHandle;
            NativeMethods.SwitchToThisWindow(handle);

            // Do SetText and Paste both
            new DeepLHelper(DataFormats.Text, text).Go();

            if (NativeMethods.GetForegroundWindow() != handle)
            {
                // UNDONE: Show DeepL for user or use Toast in win10
                Application.Current.Dispatcher.InvokeAsync(() => ModernWpf.MessageBox.Show(
                    "Didn't find DeepL client in front, will turn off DeepL extension..", "Eroge Helper"));

                _ehConfigRepository.PasteToDeepL = false;
            }
        }
    }
}