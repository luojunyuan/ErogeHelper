using Caliburn.Micro;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.ViewModel.Page
{
    public class GeneralViewModel : PropertyChangedBase
    {
        public GeneralViewModel(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

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

#pragma warning disable CS8618
        public GeneralViewModel() { }
    }
}