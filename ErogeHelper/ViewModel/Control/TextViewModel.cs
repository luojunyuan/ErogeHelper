using Caliburn.Micro;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.Windows.Media;
using ErogeHelper.Common;

namespace ErogeHelper.ViewModel.Control
{
    public class TextViewModel : PropertyChangedBase
    {
        public TextViewModel(CardViewModel cardViewModel)
        {
            CardControl = cardViewModel;
        }

        public CardViewModel CardControl { get; set; }

        private Brush _background = new SolidColorBrush();
        private BindableCollection<SingleTextItem> _sourceTextCollection = new();

        public Brush Background
        {
            get => _background;
            set { _background = value; NotifyOfPropertyChange(() => Background); }
        }

        public BindableCollection<SingleTextItem> SourceTextCollection
        {
            get => _sourceTextCollection;
            set { _sourceTextCollection = value; NotifyOfPropertyChange(() => SourceTextCollection); }
        }

        public void SearchWord(SingleTextItem clickItem)
        {
            CardControl.Word = clickItem.Text;
            CardControl.Search();
        }

#pragma warning disable CS8618
        public TextViewModel() { }
    }
}