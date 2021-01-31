using Caliburn.Micro;
using ErogeHelper.Common.Helper;
using ErogeHelper.Model;
using System;

namespace ErogeHelper.ViewModel.Pages
{
    class GeneralViewModel : PropertyChangedBase
    {
        public bool ShowAppend
        {
            get => DataRepository.ShowAppendText;
            set
            {
                DataRepository.ShowAppendText = value;
            }
        }

        public bool DeepLExtention { get => DataRepository.PasteToDeepL; set => DataRepository.PasteToDeepL = value; }

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
                NotifyOfPropertyChange(() => BrightnessSliderValue);
                brightnessHelper!.SetBrightness(DataRepository.MainProcess!.MainWindowHandle, value);
            }
        }

        private bool _brightnessSliderEnable;
        private short _brightnessSliderValue;
        readonly IAdjustScreen? brightnessHelper;

        public GeneralViewModel()
        {
            IntPtr handle = DataRepository.GameViewHandle;
            brightnessHelper = AdjustScreenBuilder.CreateAdjustScreen(handle);
            if (brightnessHelper is null)
            {
                BrightnessSliderEnable = false;
                Log.Info("Not support gdi32 brightness adjust");
            }
            else
            {
                BrightnessSliderEnable = true;
                short _min = 0;
                short _max = 0;
                short cur = 0;
                brightnessHelper.GetBrightness(handle, ref _min, ref cur, ref _max);
                BrightnessSliderValue = cur;
            }
        }
    }
}
