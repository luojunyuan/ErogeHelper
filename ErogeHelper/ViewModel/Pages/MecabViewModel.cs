using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Helper;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using System.Diagnostics;
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
        private string _mojiToken = DataRepository.MojiSessionToken;

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
                    DataRepository.Romaji = true;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = false;
                    ChangeKanaType();
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
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = true;
                    DataRepository.Katakana = false;
                    ChangeKanaType();
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
                    DataRepository.Romaji = false;
                    DataRepository.Hiragana = false;
                    DataRepository.Katakana = true;
                    ChangeKanaType();
                }
            }
        }

        private readonly MecabHelper mecabHelper = new MecabHelper();
        private void ChangeKanaType()
        {
            var tmp = new BindableCollection<SingleTextItem>();

            // This work around only takes 3~5ms it's fine! much better than WanaKana ones...
            var sentence = string.Empty;
            foreach (var sourceText in IoC.Get<TextViewModel>().SourceTextCollection)
            {
                sentence += sourceText.Text;
            }
            foreach (MecabWordInfo mecabWord in mecabHelper.MecabWordIpaEnumerable(sentence))
            {
                tmp.Add(new SingleTextItem
                {
                    Text = mecabWord.Word,
                    RubyText = mecabWord.Kana,
                    PartOfSpeed = mecabWord.PartOfSpeech,
                    TextTemplateType = DataRepository.TextTemplateConfig,
                    SubMarkColor = Utils.Hinshi2Color(mecabWord.PartOfSpeech)
                });
            }
            IoC.Get<TextViewModel>().SourceTextCollection = tmp;
        }

        public string MojiToken 
        { 
            get => _mojiToken;
            set 
            { 
                _mojiToken = value;
                NotifyOfPropertyChange(() => MojiToken);
                DataRepository.MojiSessionToken = value;
            } 
        }
    }
}
