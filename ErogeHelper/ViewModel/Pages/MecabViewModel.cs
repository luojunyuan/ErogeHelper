using Caliburn.Micro;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using WanaKanaSharp;

namespace ErogeHelper.ViewModel.Pages
{
    class MecabViewModel : PropertyChangedBase
    {
        private bool _kanaDefault = DataRepository.KanaDefault;
        private bool _kanaTop = DataRepository.KanaTop;
        private bool _kanaBottom = DataRepository.KanaBottom;
        private bool _romaji = DataRepository.Romaji;
        private bool _hiragana = DataRepository.Hiragana;
        private bool _katakana = DataRepository.Katakana;

        public bool KanaDefault
        {
            get => _kanaDefault;
            set
            {
                // Same value return
                if (value.Equals(KanaDefault))
                    return;
                // Set memory value
                _kanaDefault = value;
                // Notify all Button ? needed?
                // React change in view and save to local
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineDefault);
                    DataRepository.KanaDefault = true;
                    DataRepository.KanaTop = false;
                    DataRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaTop
        {
            get => _kanaTop;
            set
            {
                if (value.Equals(KanaTop))
                    return;
                _kanaTop = value;
                if (value) 
                { 
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaTop);
                    DataRepository.KanaDefault = false;
                    DataRepository.KanaTop = true;
                    DataRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaBottom
        {
            get => _kanaBottom;
            set
            {
                if (value.Equals(_kanaBottom))
                    return;
                _kanaBottom = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaBottom);
                    DataRepository.KanaDefault = false;
                    DataRepository.KanaTop = false;
                    DataRepository.KanaBottom = true;
                }
            }
        }
        public bool MojiVertical { get; set; }

        private static void ChangeSourceTextTemplate(TextTemplateType type)
        {
            var tmp = new BindableCollection<SingleTextItem>();
            foreach (var item in IoC.Get<TextViewModel>().SourceTextCollection)
            {
                item.TextTemplateType = type;
                tmp.Add(item);
            }
            IoC.Get<TextViewModel>().SourceTextCollection = tmp;
            DataRepository.TextTemplateConfig = type;
        }

        public bool Romaji
        {
            get => _romaji;
            set
            {
                if (value.Equals(_romaji))
                    return;
                _romaji = value;
                if (value)
                {
                    ChangeKanaType(nameof(Romaji));
                    DataRepository.Romaji = true;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = false;
                }
            }
        }
        public bool Hiragana
        { 
            get => _hiragana;
            set
            {
                if (value.Equals(_hiragana))
                    return;
                _hiragana = value;
                if (value)
                {
                    ChangeKanaType(nameof(Hiragana));
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = true;
                    DataRepository.Katakana = false;
                }
            }
        }
        public bool Katakana 
        { 
            get => _katakana;
            set
            {
                if (value.Equals(_katakana))
                    return;
                _katakana = value;
                if (value)
                {
                    ChangeKanaType(nameof(Katakana));
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = true;
                }
            }
        }

        private void ChangeKanaType(string type)
        {
            var tmp = new BindableCollection<SingleTextItem>();
            if (type == nameof(Romaji))
            {
                foreach (var item in IoC.Get<TextViewModel>().SourceTextCollection)
                {
                    item.RubyText = WanaKana.ToRomaji(item.RubyText);
                    tmp.Add(item);
                }
            }
            else if (type == nameof(Hiragana))
            {
                foreach (var item in IoC.Get<TextViewModel>().SourceTextCollection)
                {
                    // Not Implament yet
                    //item.RubyText = WanaKana.ToHiragana(item.RubyText);
                    item.RubyText = WanaKana.ToKana(WanaKana.ToRomaji(item.RubyText));
                    tmp.Add(item);
                }
            }
            else if (type == nameof(Katakana))
            {
                //foreach (var item in IoC.Get<TextViewModel>().SourceTextCollection)
                //{
                //    // Not Implament yet
                //    item.RubyText = WanaKana.ToKatakana(item.RubyText);
                //    tmp.Add(item);
                //}
                return;
            }
            IoC.Get<TextViewModel>().SourceTextCollection = tmp;
        }
    }
}
