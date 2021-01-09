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
    }
}
