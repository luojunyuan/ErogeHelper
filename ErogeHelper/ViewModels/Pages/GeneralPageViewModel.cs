using Caliburn.Micro;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.ViewModels.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ViewModels.Pages
{
    class GeneralPageViewModel : PropertyChangedBase
    {
        private bool _kanaTop = DataRepository.KanaTop;
        private bool _kanaBottom = DataRepository.KanaBottom;

        public bool ShowSource
        {
            get
            {
                return DataRepository.ShowSourceText;
            }
            set
            {
                DataRepository.ShowSourceText = value;
                IoC.Get<TextViewModel>().TextVisible = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool ShowAppend { get => DataRepository.ShowAppendText; set => DataRepository.ShowAppendText = value; }

        public bool DeepLExtention { get => DataRepository.PasteToDeepL; set => DataRepository.PasteToDeepL = value; }

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

        public void ChangeSourceTextTemplate(TextTemplateType type)
        {
            var tmp = new BindableCollection<SingleTextItem>();
            foreach (var item in IoC.Get<TextViewModel>().SourceTextCollection)
            {
                item.TextTemplateType = type;
                tmp.Add(item);
            }
            IoC.Get<TextViewModel>().SourceTextCollection = tmp;
            // cause `static` and not sington can't use IoC
            GameViewDataService.SourceTextTemplate = type;
            DataRepository.TextTemplateConfig = type;
        }
    }
}
