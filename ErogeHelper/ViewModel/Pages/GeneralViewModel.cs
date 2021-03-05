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
            set => DataRepository.ShowAppendText = value;
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
                _brightnessHelper!.SetBrightness(DataRepository.MainProcess!.MainWindowHandle, value);
            }
        }

        private bool _brightnessSliderEnable;
        private short _brightnessSliderValue;
        readonly IAdjustScreen? _brightnessHelper;

        public GeneralViewModel()
        {
            IntPtr handle = DataRepository.GameViewHandle;
            _brightnessHelper = AdjustScreenBuilder.CreateAdjustScreen(handle);
            if (_brightnessHelper is null)
            {
                BrightnessSliderEnable = false;
                Log.Info("Not support gdi32 brightness adjust");
            }
            else
            {
                BrightnessSliderEnable = true;
                short min = 0;
                short max = 0;
                short cur = 0;
                _brightnessHelper.GetBrightness(handle, ref min, ref cur, ref max);
                BrightnessSliderValue = cur;
            }
        }
    }
}
