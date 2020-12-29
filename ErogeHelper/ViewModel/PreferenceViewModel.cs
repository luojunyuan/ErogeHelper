using Caliburn.Micro;
using ErogeHelper.Common.Service;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.View.Pages;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace ErogeHelper.ViewModel
{
    class PreferenceViewModel : Screen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PreferenceViewModel));

        public HookViewModel HookSettingPage { get; set; } = IoC.Get<HookViewModel>();
        public GeneralPageViewModel GeneralSettingPage { get; set; } = IoC.Get<GeneralPageViewModel>();
        public AboutPageViewModel AboutPage { get; set; } = IoC.Get<AboutPageViewModel>();
        // https://github.com/kanryu/CaliburnApp3/blob/master/CaliburnApp3/App.xaml.cs
        // <Frame cm:Message.Attach="[Event Loaded] = [SetupNavigationService($source)]" DataContext="{x:Null}" />
        //public void SetupNavigationService(Frame frame)
        //{
        //    this.navigationService = this.container.RegisterNavigationService(frame);

        //    if (this.resume)
        //    {
        //        this.navigationService.ResumeState();
        //    }
        //}
    }
}
