using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.Model.Dictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ErogeHelper.ViewModel.Control
{
    class TextViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TextViewModel));

        private BindableCollection<SingleTextItem> _sourceTextCollection = new BindableCollection<SingleTextItem>();
        private Visibility _textVisible;

        public BindableCollection<SingleTextItem> SourceTextCollection
        {
            get => _sourceTextCollection;
            set
            {
                _sourceTextCollection = value;
                NotifyOfPropertyChange(() => SourceTextCollection);
            }
        }
        public Visibility TextVisible
        {
            get
            {
                return _textVisible;
            }
            set
            {
                _textVisible = value;
                NotifyOfPropertyChange(() => TextVisible);
            }
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public TextViewModel()
        {
            TextVisible = DataRepository.ShowSourceText ? Visibility.Visible : Visibility.Collapsed;
        }

        // make a card vm in the future
        private bool wordCardOpen = false;

        public bool WordCardOpen { get => wordCardOpen; set { wordCardOpen = value; NotifyOfPropertyChange(() => WordCardOpen); } }
        public void SearchWord(SingleTextItem clickItem)
        {
            WordCardOpen = true;
            log.Info(clickItem.Text);
        }
    }

    public class SingleTextItem
    {
        public string RubyText { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string PartOfSpeed { get; set; } = string.Empty;
        public TextTemplateType TextTemplateType { get; set; }
        public ImageSource SubMarkColor { get; set; } = DataRepository.transparentImage;
    }
}
