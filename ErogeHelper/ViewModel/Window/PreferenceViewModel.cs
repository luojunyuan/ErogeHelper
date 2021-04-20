using System;
using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.ViewModel.Window
{
    public class PreferenceViewModel
    {
        public PreferenceViewModel(
            GeneralViewModel generalViewModel,
            MeCabViewModel meCabViewModel,
            HookViewModel hookViewModel,
            TransViewModel transViewModel,
            AboutViewModel aboutViewModel)
        {
            GeneralViewModel = generalViewModel;
            MeCabViewModel = meCabViewModel;
            HookViewModel = hookViewModel;
            TransViewModel = transViewModel;
            AboutViewModel = aboutViewModel;
        }

        public GeneralViewModel GeneralViewModel;
        public MeCabViewModel MeCabViewModel;
        public HookViewModel HookViewModel;
        public TransViewModel TransViewModel;

        public AboutViewModel AboutViewModel;
    }
}