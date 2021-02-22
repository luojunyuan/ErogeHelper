using Caliburn.Micro;
using ErogeHelper.Common.Converter;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model;
using ErogeHelper.Model.Translator;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ErogeHelper.ViewModel.Pages
{
    class TransViewModel : PropertyChangedBase, IHandle<RefreshTranslatorsListMessage>
    {
        #region Fields
        private Languages _selectedSrcLang;
        private Languages _selectedTarLang;
        private BindableCollection<LanguageItem> _targetLanguageList;
        #endregion

        private readonly IEventAggregator eventAggregator;

        public TransViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.SubscribeOnUIThread(this);

            SrcLanguageList = SrcLanguageListInit();
            SelectedSrcLang = DataRepository.TransSrcLanguage;

            TargetLanguageList = TargetLanguageListRefresh(out _);
            SelectedTarLang = DataRepository.TransTargetLanguage;

            RefreshTranslatorList();
        }

        public BindableCollection<LanguageItem> SrcLanguageList { get; }

        public Languages SelectedSrcLang
        { get => _selectedSrcLang; set { _selectedSrcLang = value; NotifyOfPropertyChange(() => SelectedSrcLang); } }

        public void SrcLanguageChanged()
        {
            DataRepository.TransSrcLanguage = SelectedSrcLang;

            var markTarLang = SelectedTarLang;
            TargetLanguageList = TargetLanguageListRefresh(out Dictionary<Languages, bool> tmpLangDict);
            // To avoid if last target language not include in new target language list
            if (!tmpLangDict.ContainsKey(markTarLang))
            {
                var enumerator = tmpLangDict.GetEnumerator();
                enumerator.MoveNext();
                var firstLang = enumerator.Current.Key;
                Log.Debug(firstLang.ToString());
                SelectedTarLang = firstLang;
                DataRepository.TransTargetLanguage = SelectedTarLang;
            }
            else
            {
                SelectedTarLang = markTarLang;
            }

            RefreshTranslatorList(true);
        }

        public BindableCollection<LanguageItem> TargetLanguageList
        { get => _targetLanguageList; set { _targetLanguageList = value; NotifyOfPropertyChange(() => TargetLanguageList); } }

        public Languages SelectedTarLang
        { get => _selectedTarLang; set { _selectedTarLang = value; NotifyOfPropertyChange(() => SelectedTarLang); } }

        public void TargetLanguageChanged()
        {
            DataRepository.TransTargetLanguage = SelectedTarLang;
            RefreshTranslatorList(true);
        }

        public BindableCollection<TransItem> TranslatorList { get; set; } = new();

        public async void SetTranslatorDialog(string translatorName) =>
            await eventAggregator.PublishOnUIThreadAsync(
                                                    new OpenApiKeyDialogMessage { TranslatorName = translatorName });

        private void RefreshTranslatorList(bool reset = false)
        {
            if (reset)
            {
                foreach (var translator in TranslatorManager.GetAll)
                {
                    translator.IsEnable = false;
                }
            }
            TranslatorList.Clear();

            foreach (var translator in TranslatorManager.GetAll)
            {
                if (translator.SupportSrcLang.Contains(SelectedSrcLang) && translator.SupportDesLang.Contains(SelectedTarLang))
                {
                    var translatorItem = new TransItem()
                    {
                        CanbeEnable = translator.UnLock,
                        Enable = translator.IsEnable,
                        IconPath = translator.IconPath,
                        TransName = translator.Name,
                        CanEdit = !translator.NeedKey,
                    };
                    TranslatorList.Add(translatorItem);
                }
            }
        }

        private BindableCollection<LanguageItem> SrcLanguageListInit()
        {
            BindableCollection<LanguageItem> langList = new();
            Dictionary<Languages, bool> tmpMark = new();
            foreach (var translator in TranslatorManager.GetAll)
            {
                foreach (var lang in translator.SupportSrcLang)
                {
                    if (!tmpMark.ContainsKey(lang))
                    {
                        langList.Add(new LanguageItem() { LangEnum = lang, Language = lang.ToString() });
                        tmpMark[lang] = true;
                    }
                }
            }
            return langList;
        }

        private BindableCollection<LanguageItem> TargetLanguageListRefresh(out Dictionary<Languages, bool> tmpMark)
        {
            BindableCollection<LanguageItem> langList = new();
            tmpMark = new();
            foreach (var translator in TranslatorManager.GetAll)
            {
                if (translator.SupportSrcLang.Contains(SelectedSrcLang))
                {
                    foreach (var lang in translator.SupportDesLang)
                    {
                        if (!tmpMark.ContainsKey(lang) && lang != SelectedSrcLang)
                        {
                            langList.Add(new LanguageItem() { LangEnum = lang, Language = lang.ToString() });
                            tmpMark[lang] = true;
                        }
                    }
                }
            }
            return langList;
        }

        public async Task HandleAsync(RefreshTranslatorsListMessage message, CancellationToken cancellationToken) =>
            await Task.Run(() => RefreshTranslatorList());
    }

    class TransItem : PropertyChangedBase
    {
        private bool _enable;

        public bool CanbeEnable { get; set; }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                if (!TransName.Equals(string.Empty))
                {
                    TranslatorManager.GetTranslatorByName(TransName).IsEnable = value;
                }
            }
        }

        public string IconPath { get; set; } = string.Empty;

        public string TransName { get; set; } = string.Empty;

        public bool CanEdit { get; set; }
    }

    class LanguageItem
    {
        public string Language { get; set; } = string.Empty;

        public Languages LangEnum { get; set; }
    }
}
