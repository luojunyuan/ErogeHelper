using Caliburn.Micro;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;

namespace ErogeHelper.ViewModel.Pages
{
    class MecabViewModel : PropertyChangedBase
    {
        private bool _kanaDefault = DataRepository.KanaDefault;
        private bool _kanaTop = DataRepository.KanaTop;
        private bool _kanaBottom = DataRepository.KanaBottom;
        public bool KanaDefault
        {
            get => _kanaDefault;
            set
            {
                if (value.Equals(KanaDefault))
                    return;
                _kanaDefault = value;
                NotifyOfPropertyChange(() => KanaDefault);
                if (value)
                    ChangeSourceTextTemplate(TextTemplateType.OutLineDefault);
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
                NotifyOfPropertyChange(() => KanaTop);
                if (value)
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaTop);
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
                NotifyOfPropertyChange(() => KanaBottom);
                if (value)
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaBottom);
            }
        }
        public bool MojiVertical { get; set; }
        public bool Romaji { get; set; }
        public bool Furikana { get; set; }
        public bool Katakana { get; set; }

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
    }
}
