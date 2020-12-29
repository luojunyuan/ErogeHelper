using Caliburn.Micro;
using ErogeHelper.ViewModel.Pages;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModel
{
    class HookConfigViewModel : Screen
    {
        public HookViewModel HookSettingPage { get; set; } = IoC.Get<HookViewModel>();

        public Frame ContentFrame { get; set; } = new Frame();

        public HookConfigViewModel()
        {
            ContentFrame.Navigate(typeof(View.Pages.HookPage), null);
        }

        public async Task TryClose()
        {
            await TryCloseAsync().ConfigureAwait(false);
        }
    }
}
