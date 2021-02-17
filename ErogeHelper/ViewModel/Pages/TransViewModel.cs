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
            SelectedSrcLang = DataRepository.TransSrcLanguage;
            SelectedTarLang = DataRepository.TransTargetLanguage;
            RefreshTranslatorList();
        }

        public BindableCollection<LanguageItem> SrcLanguageList { get; } = new()
        {
            new() { Language = Languages.日本語.ToString(), LangEnum = Languages.日本語 },
            new() { Language = Languages.English.ToString(), LangEnum = Languages.English },
        };

        public Languages SelectedSrcLang { get => _selectedSrcLang; set { _selectedSrcLang = value; NotifyOfPropertyChange(() => SelectedSrcLang); } }

        public void SrcLanguageChanged()
        {
            DataRepository.TransSrcLanguage = SelectedSrcLang;

            //RefreshTargetLanguageList();
            RefreshTranslatorList();
        }

        public BindableCollection<LanguageItem> TargetLanguageList { get; } = new()
        {
            new() { Language = Languages.简体中文.ToString(), LangEnum = Languages.简体中文 },
            new() { Language = Languages.English.ToString(), LangEnum = Languages.English },
        };

        public Languages SelectedTarLang { get => _selectedTarLang; set { _selectedTarLang = value; NotifyOfPropertyChange(() => SelectedTarLang); } }

        public void TargetLanguageChanged()
        {
            DataRepository.TransTargetLanguage = SelectedTarLang;

            RefreshTranslatorList();
        }

        public BindableCollection<TransItem> TranslatorList { get; set; } = new();

        private void RefreshTranslatorList()
        {
            TranslatorList.Clear();

            foreach (var translator in TranslatorManager.GetAll)
            {
                if (translator.SupportSrcLang.Contains(SelectedSrcLang) && translator.SupportDesLang.Contains(SelectedTarLang))
                {
                    var translatorItem = new TransItem()
                    { CanbeSelected = false, Enable = translator.IsEnable, TransName = translator.Name };
                    TranslatorList.Add(translatorItem);
                }
            }
        }

        private void RefreshTargetLanguageList()
        {
            var markLangItem = new LanguageItem() { Language = SelectedTarLang.ToString(), LangEnum = SelectedTarLang };
            TargetLanguageList.Clear();

            Languages markFirstPositionTargetlang = Languages.Auto;
            foreach(var translator in TranslatorManager.GetAll)
            {
                if (translator.SupportSrcLang.Contains(SelectedSrcLang))
                {
                    foreach (var lang in translator.SupportDesLang)
                    {
                        var item = new LanguageItem() { LangEnum = lang, Language = lang.ToString() };
                        if (!TargetLanguageList.Contains(item))
                        {
                            markFirstPositionTargetlang = lang;
                            TargetLanguageList.Add(item);
                        }
                    }
                }
            }
            if (TargetLanguageList.Contains(markLangItem))
            {
                SelectedTarLang = markLangItem.LangEnum;
            }
            else
            {
                SelectedTarLang = markFirstPositionTargetlang;
            }
        }

        //public BindableCollection<string> SrcLanguageList { get; set; } = new() { "Japanese" };
        //public BindableCollection<string> TargetLanguageList { get; set; } = new() { "Chinese Simplify" };

        //public ComboBoxItem SrcLangItem
        //{
        //    get => _srcLangItem;
        //    set
        //    {
        //        _srcLangItem = value;
        //        NotifyOfPropertyChange(() => SrcLangItem);
        //    }
        //}
        //public string SrcLang
        //{
        //    get => _srcLang; set
        //    {
        //        _srcLang = value;
        //        NotifyOfPropertyChange(() => SrcLang);
        //    }
        //}
        //public int SrcLangIndex
        //{
        //    get => _srcLangIndex; set { _srcLangIndex = value; NotifyOfPropertyChange(() => SrcLangIndex); }
        //}
        //public Languages? TarLang
        //{
        //    get => _tarLang;
        //    set
        //    {
        //        _tarLang = value;
        //        NotifyOfPropertyChange(() => TarLang);
        //    }
        //}


        //public void TargetLanguageChanged()
        //{
        //    Log.Debug("right");
        //}

        //public TransViewModel()
        //{
        //    if (DataRepository.TransSrcLanguage == Languages.Japenese)
        //    {
        //        SrcLangIndex = 0;
        //    }
        //    else if (DataRepository.TransSrcLanguage == Languages.English)
        //    {
        //        SrcLangIndex = 1;
        //    }
        //    var srcLangText = new LanguageEnumToStringConverter().Convert(DataRepository.TransSrcLanguage, null!, null!, null!) as string;
        //    SrcLang = srcLangText!;
        //    SrcLangItem = new ComboBoxItem() { Content = srcLangText };
        //    TarLang = DataRepository.TransTargetLanguage;
        //    //RefreshTranslatorList();
        //}


    }

    class TransItem
    {
        public bool CanbeSelected { get; set; }
        public bool Enable { get; set; }
        public string TransName { get; set; } = string.Empty;
    }

    class LanguageItem
    {
        public string Language { get; set; } = string.Empty;

        public Languages LangEnum { get; set; }
    }
}
