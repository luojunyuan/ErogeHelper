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
        public HookViewModel HookPage { get; set; } = IoC.Get<HookViewModel>();

        public async Task TryClose()
        {
            await TryCloseAsync().ConfigureAwait(false);
        }
    }
}
