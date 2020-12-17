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
        public HookSettingPageViewModel HookSettingPage { get; set; } = IoC.Get<HookSettingPageViewModel>();

        public Frame ContentFrame { get; set; } = new Frame();

        public HookConfigViewModel()
        {
            ContentFrame.Navigate(typeof(Views.Pages.HookSettingPage), null);
        }

        public async Task TryClose()
        {
            await TryCloseAsync();
        }
    }
}
