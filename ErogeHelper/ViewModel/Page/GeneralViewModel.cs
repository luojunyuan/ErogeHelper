using Caliburn.Micro;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.ViewModel.Page
{
    public class GeneralViewModel : PropertyChangedBase
    {
        public GeneralViewModel(
            EhConfigRepository ehConfigRepository, 
            EhGlobalValueRepository ehGlobalValueRepository,
            IAdjustScreenBrightness brightnessHelper)
        {
            _ehConfigRepository = ehConfigRepository;
            _brightnessHelper = brightnessHelper;

            var canAdjustBrightness = _brightnessHelper.GetBrightness(out var currentBrightness, 
                                                                           out var minBrightness, 
                                                                           out var maxBrightness);
            if (canAdjustBrightness)
            {
                MinBrightness = minBrightness;
                MaxBrightness = maxBrightness;
                BrightnessSliderEnable = true;
                BrightnessSliderValue = currentBrightness;
            }
            else
            {
                BrightnessSliderEnable = false;
                Log.Info("Not support gdi32 brightness adjust");
            }
        }

        private readonly IAdjustScreenBrightness _brightnessHelper;
        private readonly EhConfigRepository _ehConfigRepository;

        private bool _brightnessSliderEnable;
        private short _brightnessSliderValue;

        public bool ShowAppend
        {
            get => _ehConfigRepository.ShowAppendText;
            set => _ehConfigRepository.ShowAppendText = value;
        }

        public bool DeepLExtention
        {
            get => _ehConfigRepository.PasteToDeepL; 
            set => _ehConfigRepository.PasteToDeepL = value;
        }

        public bool BrightnessSliderEnable
        {
            get => _brightnessSliderEnable;
            set { _brightnessSliderEnable = value; NotifyOfPropertyChange(() => BrightnessSliderEnable); }
        }

        public short BrightnessSliderValue
        {
            get => _brightnessSliderValue;
            set
            {
                _brightnessSliderValue = value;
                _brightnessHelper.SetBrightness(value);
                NotifyOfPropertyChange(() => BrightnessSliderValue);
            }
        }

        public short MinBrightness { get; set; }  

        public short MaxBrightness { get; set; }  

#pragma warning disable CS8618
        public GeneralViewModel() { }
    }
}