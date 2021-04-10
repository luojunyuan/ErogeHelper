using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.ViewModel.Window
{
    public class PreferenceViewModel
    {
        public PreferenceViewModel(
            GeneralViewModel generalViewModel,
            MeCabViewModel meCabViewModel,
            HookViewModel hookViewModel,
            AboutViewModel aboutViewModel)
        {
            GeneralViewModel = generalViewModel;
            MeCabViewModel = meCabViewModel;
            HookViewModel = hookViewModel;
            AboutViewModel = aboutViewModel;
        }

        public readonly GeneralViewModel GeneralViewModel;
        public readonly MeCabViewModel MeCabViewModel;
        public readonly HookViewModel HookViewModel;

        public readonly AboutViewModel AboutViewModel;
    }
}