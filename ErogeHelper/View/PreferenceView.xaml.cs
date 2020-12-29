using ErogeHelper.View.Pages;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ErogeHelper.View
{
    /// <summary>
    /// PreferenceView.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceView : Window
    {
        public PreferenceView()
        {
            InitializeComponent();

            ContentFrame.Navigated += OnNavigated;
            PageNavigate("hook_setting", new EntranceNavigationTransitionInfo());
        }

        private readonly List<(string Tag, Type PageType)> pages = new()
        {
            ("hook_setting", typeof(HookPage)),
            ("about", typeof(AboutPage)),
            ("general_setting", typeof(GeneralPage)),
        };

        private void NavView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString()!;
                PageNavigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void PageNavigate(string navItemTag, NavigationTransitionInfo info)
        {
            var item = pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;

            // if not same page
            if (pageType != null && ContentFrame!.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType, null, info);
                //ContentFrame.DataContext = item.DataContext;
            }
        }

        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
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

                HeaderBlock.Text =
                    ((NavigationViewItem)NavView.SelectedItem!).Content?.ToString();
            }
        }
    }
}
