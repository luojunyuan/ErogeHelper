using Caliburn.Micro;
using ErogeHelper.Common.Selector;
using ErogeHelper.Model;
using ErogeHelper.View.Control;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ErogeHelper.ViewModel.Control
{
    class TextViewModel : Screen
    {
        private BindableCollection<SingleTextItem> _sourceTextCollection = new BindableCollection<SingleTextItem>();
        private Brush _background = new SolidColorBrush();

        public Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                NotifyOfPropertyChange(() => Background);
            }
        }

        public BindableCollection<SingleTextItem> SourceTextCollection
        {
            get => _sourceTextCollection;
            set
            {
                _sourceTextCollection = value;
                NotifyOfPropertyChange(() => SourceTextCollection);
            }
        }

        public CardViewModel CardControl { get; set; }

        public void SearchWord(SingleTextItem clickItem)
        {
            if (clickItem.SubMarkColor.ToString() == DataRepository.transparentImage.ToString())
                return;

            CardControl.Word = clickItem.Text;
            CardControl.StartupSearch();
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

        public string Ruby { get; set; } = string.Empty;
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
