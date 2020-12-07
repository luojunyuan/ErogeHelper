using Caliburn.Micro;
using ErogeHelper_Core.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.ViewModels
{
    class HookConfigViewModel : PropertyChangedBase
    {
        public HookSettingPageViewModel HookSettingPage { get; set; } = IoC.Get<HookSettingPageViewModel>();
    }
}
