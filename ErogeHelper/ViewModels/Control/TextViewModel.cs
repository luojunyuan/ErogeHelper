using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ErogeHelper.ViewModels.Control
{
    class TextViewModel : PropertyChangedBase
    {
        private BindableCollection<SingleTextItem> sourceTextCollection = new BindableCollection<SingleTextItem>();
        private Visibility textVisible;

        public BindableCollection<SingleTextItem> SourceTextCollection
        {
            get => sourceTextCollection;
            set
            {
                sourceTextCollection = value;
                NotifyOfPropertyChange(() => SourceTextCollection);
            }
        }
        public Visibility TextVisible 
        { 
            get
            {
                return textVisible;
            }
            set 
            { 
                textVisible = value;
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
    }

    public class SingleTextItem
    {
        public string RubyText { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string PartOfSpeed { get; set; } = string.Empty;
        public TextTemplateType TextTemplateType { get; set; }
        public ImageSource SubMarkColor { get; set; } = Utils.LoadBitmapFromResource("Assets/transparent.png");
    }
}
