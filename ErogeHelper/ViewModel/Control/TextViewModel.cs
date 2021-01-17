using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.Model.Dictionary;
using ErogeHelper.ViewModel.Pages;
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
        private Visibility _textVisible = IoC.Get<GeneralViewModel>().ShowSource ? Visibility.Visible : Visibility.Collapsed;

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

        public CardViewModel CardFlyout { get; set; }

        public void SearchWord(SingleTextItem clickItem)
        {
            if (clickItem.SubMarkColor.ToString() == DataRepository.transparentImage.ToString())
                return;

            log.Info(clickItem.Text);
            CardFlyout.Word = clickItem.Text;
        }

        public TextViewModel(CardViewModel cardViewModel)
        {
            CardFlyout = cardViewModel;
        }
    }

    public class SingleTextItem
    {
        private ImageSource _subMarkColor = DataRepository.transparentImage;

        public string RubyText { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string PartOfSpeed { get; set; } = string.Empty;
        public TextTemplateType TextTemplateType { get; set; }
        public ImageSource SubMarkColor 
        { 
            get => _subMarkColor;
            set 
            { 
                _subMarkColor = value;
                Application.Current.Dispatcher.InvokeAsync(
                    () => CanBeSearch = (value.ToString() == DataRepository.transparentImage.ToString()) ? false : true);
            }
        }
        public bool CanBeSearch { get; set; }
    }
}
