using Caliburn.Micro;
using ErogeHelper.Common.Converter;
using ErogeHelper.Model;
using ErogeHelper.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ErogeHelper.ViewModel.Pages
{
    class TransViewModel : PropertyChangedBase
    {
        #region Fields
        private Languages _selectedSrcLang;
        private Languages _selectedTarLang;
        #endregion

        public TransViewModel()
        {
            SrcLanguageList = SrcLanguageListInit();
            SelectedSrcLang = DataRepository.TransSrcLanguage;

            TargetLanguageList = TargetLanguageListRefresh(out _);
            SelectedTarLang = DataRepository.TransTargetLanguage;

            RefreshTranslatorList();
        }

        public BindableCollection<LanguageItem> SrcLanguageList { get; }

        public Languages SelectedSrcLang { get => _selectedSrcLang; set { _selectedSrcLang = value; NotifyOfPropertyChange(() => SelectedSrcLang); } }

        public void SrcLanguageChanged()
        {
            DataRepository.TransSrcLanguage = SelectedSrcLang;

            var markTarLang = SelectedTarLang;
            TargetLanguageListRefresh(out Dictionary<Languages, bool> tmpLangDict);
            if (!tmpLangDict.ContainsKey(markTarLang))
            {
                Languages firstLang = tmpLangDict.GetEnumerator().Current.Key;
                Log.Debug(firstLang.ToString());
                SelectedTarLang = firstLang;
                DataRepository.TransTargetLanguage = SelectedTarLang;
            }
            RefreshTranslatorList(true);
        }

        public BindableCollection<LanguageItem> TargetLanguageList { get; }

        public Languages SelectedTarLang { get => _selectedTarLang; set { _selectedTarLang = value; NotifyOfPropertyChange(() => SelectedTarLang); } }

        public void TargetLanguageChanged()
        {
            DataRepository.TransTargetLanguage = SelectedTarLang;
            RefreshTranslatorList(true);
        }

        public BindableCollection<TransItem> TranslatorList { get; set; } = new();

        public void SetTranslatorDialog(string translatorName)
        {
            Log.Debug(translatorName);
        }

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
                        CanbeSelected = !translator.NeedKey,
                        Enable = translator.IsEnable,
                        TransName = translator.Name,
                        DialogHasExist = true,
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
                foreach(var lang in translator.SupportSrcLang)
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
                        if (!tmpMark.ContainsKey(lang))
                        {
                            langList.Add(new LanguageItem() { LangEnum = lang, Language = lang.ToString() });
                            tmpMark[lang] = true;
                        }
                    }
                }
            }
            return langList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status">true enable, false disable</param>
        private void SwitchDialogButtonEnable(bool status)
        {
            foreach (var item in TranslatorList)
            {
                item.DialogHasExist = status;
            }
        }
    }

    class TransItem : PropertyChangedBase
    {
        private bool _enable;
        private bool dialogHasExist;

        public bool CanbeSelected { get; set; }
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
        public string TransName { get; set; } = string.Empty;

        public bool DialogHasExist 
        {
            get => dialogHasExist;
            set 
            { 
                dialogHasExist = value;
                NotifyOfPropertyChange(() => DialogHasExist);
            } 
        }
    }

    class LanguageItem
    {
        public string Language { get; set; } = string.Empty;

        public Languages LangEnum { get; set; }
    }
}
