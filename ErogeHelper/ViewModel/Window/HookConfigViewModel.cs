using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.ViewModel.Window
{
    public class HookConfigViewModel
    {
        public HookConfigViewModel(HookViewModel hookViewModel)
        {
            HookPage = hookViewModel;
        }

        public HookViewModel HookPage { get; set; }
    }
}