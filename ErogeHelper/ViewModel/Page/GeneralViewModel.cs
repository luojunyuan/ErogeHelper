using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.ViewModel.Window;

namespace ErogeHelper.ViewModel.Page
{
    public class GeneralViewModel : PropertyChangedBase
    {
        public GeneralViewModel(EhConfigRepository ehConfigRepository, IEventAggregator eventAggregator)
        {
            _ehConfigRepository = ehConfigRepository;
            _eventAggregator = eventAggregator;
        }

        private readonly EhConfigRepository _ehConfigRepository;
        private readonly IEventAggregator _eventAggregator;

        public bool OutsideWindow
        {
            get => _ehConfigRepository.UseOutsideWindow;
            set
            {
                if (value)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new InsideViewTextVisibleMessage {IsShowed = false});
                    _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"));
                }
                else
                {
                    _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Hide, null, "OutsideView"));
                    _eventAggregator.PublishOnUIThreadAsync(new InsideViewTextVisibleMessage {IsShowed = true});
                }
                _ehConfigRepository.UseOutsideWindow = value;
            }
        }

        public bool DeepLExtention
        {
            get => _ehConfigRepository.PasteToDeepL;
            set => _ehConfigRepository.PasteToDeepL = value;
        }

        public void SetDefaultEhConfig()
        {
            DeepLExtention = false;
            NotifyOfPropertyChange(() => DeepLExtention);
            _ehConfigRepository.ClearConfig();
        }

        // UNDONE: Auto inject user hook code

#pragma warning disable CS8618
        public GeneralViewModel() { }
    }
}