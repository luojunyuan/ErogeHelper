using Caliburn.Micro;
using ErogeHelper.Common.Selector;
using ErogeHelper.Common.Service;
using ErogeHelper.Model;
using ErogeHelper.ViewModel.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.ViewModel.Pages
{
    class GeneralViewModel : PropertyChangedBase
    {
        public bool ShowSource
        {
            get => DataRepository.ShowSourceText;
            set
            {
                DataRepository.ShowSourceText = value;
                IoC.Get<TextViewModel>().TextVisible = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool ShowAppend 
        {
            get => DataRepository.ShowAppendText; 
            set 
            {
                DataRepository.ShowAppendText = value;
                // IoC.Get<TextViewModel>().TextVisible = value ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        public bool DeepLExtention { get => DataRepository.PasteToDeepL; set => DataRepository.PasteToDeepL = value; }

        //// TENDO: Improve these
        //short minBrightness = 0; //22
        //short curBrightness = 0;
        //short maxBrightness = 0; //85
        //public bool CanBrightnessDown() => true;
        //public void BrightnessDown()
        //{
        //    bool result = brightnessHelper!.SetBrightness(DataRepository.MainProcess!.MainWindowHandle, --curBrightness);
        //    log.Info($"Current brightness: {curBrightness} ({minBrightness}-{maxBrightness})");
        //}
        //public bool CanBrightnessUp() => true;
        //public void BrightnessUp()
        //{
        //    bool result = brightnessHelper!.SetBrightness(DataRepository.MainProcess!.MainWindowHandle, ++curBrightness);
        //    log.Info($"Current brightness: {curBrightness} ({minBrightness}-{maxBrightness})");
        //}


        //IAdjustScreen? brightnessHelper;

        //protected override void OnViewLoaded(object view)
        //{
        //    base.OnViewLoaded(view);

        //    brightnessHelper = AdjustScreenBuilder.CreateAdjustScreen((Window)view);
        //    if (brightnessHelper == null)
        //    {
        //        log.Info("Not support brightness adjust");
        //    }
        //    else
        //    {
        //        // use game's handle or EH GameView's new windowsInterrupter(GetView()).handle
        //        IntPtr handle = DataRepository.MainProcess!.MainWindowHandle;

        //        brightnessHelper.GetBrightness(handle,
        //            ref minBrightness,
        //            ref curBrightness,
        //            ref maxBrightness);
        //        log.Info($"Current brightness: {curBrightness} ({minBrightness}-{maxBrightness})");
        //    }
        //}
    }
}
