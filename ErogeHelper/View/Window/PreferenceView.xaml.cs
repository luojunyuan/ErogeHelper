using ErogeHelper.View.Page;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using WindowPage = System.Windows.Controls.Page;

namespace ErogeHelper.View.Window
{
    /// <summary>
    /// PreferenceView.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceView
    {
        public PreferenceView()
        {
            InitializeComponent();

            _pagesList =  new List<(string Tag, WindowPage PageInstance)>()
            {
                ("general", _generalPage),
                ("mecab", _mecabPage),
                ("hook", _hookPage),
                //("trans", typeof(TransPage)),

                ("about", _aboutPage),
            };
            ContentFrame.Navigated += OnNavigated;
            Loaded += (_, _) => PageNavigate("general", new EntranceNavigationTransitionInfo());
        }

        private readonly GeneralPage _generalPage = new();
        private readonly MeCabPage _mecabPage = new();
        private readonly HookPage _hookPage = new();
        private readonly AboutPage _aboutPage = new();

        private readonly List<(string Tag, WindowPage PageInstance)> _pagesList;

        private readonly List<(string Tag, Type PageType)> _pages = new()
        {
            ("general", typeof(GeneralPage)),
            ("mecab", typeof(MeCabPage)),
            ("hook", typeof(HookPage)),
            //("trans", typeof(TransPage)),

            ("about", typeof(AboutPage)),
        };

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is null) 
                return;

            var navItemTag = args.SelectedItemContainer.Tag.ToString()!;
            PageNavigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }

        private void PageNavigate(string navItemTag, NavigationTransitionInfo info)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            var instanceItem = _pagesList.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;
            WindowPage pageInstance = instanceItem.PageInstance;

            // if not same page
            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                // Clear Journal info (non-use)
                while (ContentFrame.CanGoBack)
                {
                    ContentFrame.RemoveBackEntry();
                }
                // https://github.com/Kinnara/ModernWpf/issues/329
                ContentFrame.Navigate(pageType, null, info);
                //ContentFrame.Navigate(pageInstance);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
            Type sourcePageType = ContentFrame.SourcePageType;
            // Set header text
            if (sourcePageType is not null)
            {
                var item = _pages.FirstOrDefault(p => p.PageType == sourcePageType);

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
