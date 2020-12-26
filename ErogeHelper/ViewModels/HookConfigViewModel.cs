using Caliburn.Micro;
using ErogeHelper.ViewModels.Pages;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.ViewModels
{
    class HookConfigViewModel : Screen
    {
        public HookPageViewModel HookSettingPage { get; set; } = IoC.Get<HookPageViewModel>();

        public Frame ContentFrame { get; set; } = new Frame();

        public HookConfigViewModel()
        {
            ContentFrame.Navigate(typeof(Views.Pages.HookPage), null);
        }

        public async Task TryClose()
        {
            await TryCloseAsync().ConfigureAwait(false);
        }
    }
}
