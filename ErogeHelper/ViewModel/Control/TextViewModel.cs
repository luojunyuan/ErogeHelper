using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.Model.Dictionary;
using ErogeHelper.View.Control;
using ErogeHelper.ViewModel.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ErogeHelper.ViewModel.Control
{
    class TextViewModel : Screen
    {
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

        public CardViewModel CardControl { get; set; }

        public async void SearchWord(SingleTextItem clickItem)
        {
            if (clickItem.SubMarkColor.ToString() == DataRepository.transparentImage.ToString())
                return;

            Log.Info($"Click {clickItem.Text}");
            // Clear data first
            CardControl.Word = clickItem.Text;
            CardControl.ClearData();
            await CardControl.MojiSearchAsync().ConfigureAwait(false);
        }

        public void CloseCardControl()
        {
            var view = GetView() as TextControl;
            var popup = view!.Resources["CardPopup"] as Popup;
            popup!.IsOpen = false;
        }

        public TextViewModel(CardViewModel cardViewModel)
        {
            CardControl = cardViewModel;
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
                    () => CanBeSearch = !value.ToString().Equals(DataRepository.transparentImage.ToString()));
            }
        }
        public bool CanBeSearch { get; set; }
    }
}
