using ErogeHelper.View.Page;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Caliburn.Micro;
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

            ContentFrame.Navigated += OnNavigated;
            Loaded += (_, _) =>
            {
                _pagesList = new List<(string Tag, WindowPage PageInstance)>()
                {
                    ("general", new GeneralPage()),
                    ("mecab", new MeCabPage()),
                    ("hook", new HookPage()),
                    //("trans", typeof(TransPage)),

                    ("about", new AboutPage()),
                };

                PageNavigate("general");
            };
        }

        private List<(string Tag, WindowPage PageInstance)>? _pagesList;

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
            PageNavigate(navItemTag);
        }

        private void PageNavigate(string navItemTag)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            var instanceItem = _pagesList?.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;
            WindowPage pageInstance = (instanceItem ?? throw new InvalidOperationException()).PageInstance;

            // if not same page
            if (pageType != null && ContentFrame.SourcePageType != pageType)
            {
                // Clear Journal info (non-use)
                while (ContentFrame.CanGoBack)
                {
                    ContentFrame.RemoveBackEntry();
                }
                // https://github.com/Kinnara/ModernWpf/issues/329
                ContentFrame.Navigate(pageInstance, null);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
            Type sourcePageType = ContentFrame.SourcePageType;
            // Set header text
            if (sourcePageType is not null)
            {
                var (tag, _) = _pages.FirstOrDefault(p => p.PageType == sourcePageType);

                NavView.SelectedItem = NavView.FooterMenuItems
                    .OfType<NavigationViewItem>().
                    FirstOrDefault(n => n.Tag.Equals(tag)) ??
                    NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag.Equals(tag));

                HeaderBlock.Text =
                    ((NavigationViewItem)NavView.SelectedItem!).Content?.ToString();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            IEventAggregator eventAggregator = IoC.Get<IEventAggregator>();
            foreach (var (_, instance) in _pagesList ?? throw new InvalidOperationException())
            {
                eventAggregator.Unsubscribe(instance);
            }
        }
    }
}
