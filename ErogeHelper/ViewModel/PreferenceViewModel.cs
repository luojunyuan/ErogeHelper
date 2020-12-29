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
using Autofac;

namespace ErogeHelper.ViewModel
{
    class PreferenceViewModel : Screen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PreferenceViewModel));

        public Frame ContentFrame { get; set; } = new Frame();
        public NavigationViewItem SelectedViewItem { get; set; } = new NavigationViewItem();
        public string HeaderBlock 
        { 
            get => headerBlock;
            set 
            { 
                headerBlock = value;
                NotifyOfPropertyChange(()=>HeaderBlock);
            } 
        }
        public void SetupNavigationService(Frame frame)
        {
            // 我根本没有用绑定、全靠床过来的frame
            // 先不论用不用上，假设没绑定，我之后也要用这个ContentFrame进行操作
            ContentFrame = frame; // 这个理应不用也能绑上，用的话还应该加Notify
            // frame 只有高度宽度，还没有内容
            ContentFrame.Navigated += OnNavigated;
            PageNavigate("hook_setting", new EntranceNavigationTransitionInfo());
        }

        private readonly List<(string Tag, Type PageType)> pages = new()
        {
            ("hook_setting", typeof(HookPage)),
            ("about", typeof(AboutPage)),
            ("general_setting", typeof(GeneralPage)),
        };

        public void ViewItemSelectionChanged(NavigationView NavView, NavigationViewSelectionChangedEventArgs args)
        {
            this.NavView = NavView; // 首次加载还没发生
            if (args.SelectedItem != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString()!;
                PageNavigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        /// <summary>
        /// Use Frame.Navigate() by page type
        /// </summary>
        /// <param name="navItemTag"></param>
        /// <param name="info"></param>
        private void PageNavigate(string navItemTag, NavigationTransitionInfo info)
        {
            var item = pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;

            if (pageType != null && ContentFrame!.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType, null, info);
                //ContentFrame.DataContext = IoC.Get<HookViewModel>();
            }
        }

        private NavigationView NavView = new NavigationView();
        private string headerBlock = string.Empty;

        /// <summary>
        /// Set page header info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Type sourcePageType = ContentFrame.SourcePageType;
            if (sourcePageType != null)
            {
                var item = pages.FirstOrDefault(p => p.PageType == sourcePageType);

                NavView.SelectedItem = NavView.FooterMenuItems
                    .OfType<NavigationViewItem>().
                    FirstOrDefault(n => n.Tag.Equals(item.Tag)) ??
                    NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag.Equals(item.Tag));

                if (NavView.SelectedItem != null)
                {
                    HeaderBlock = ((NavigationViewItem)NavView.SelectedItem!).Content?.ToString();
                }
                else
                {
                    HeaderBlock = "Hooksetting";
                }
            }
        }

        //MenuItemsSource="{Binding MenuItems}" FooterMenuItemsSource="{Binding FooterMenuItems}"
        //public BindableCollection<SettingMenuItem> MenuItems { get; set; } = new BindableCollection<SettingMenuItem>();
        //public BindableCollection<SettingMenuItem> FooterMenuItems { get; set; } = new BindableCollection<SettingMenuItem>();
    }
}
