using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Platform;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.View.MainGame.AssistiveMenu;
using ErogeHelper.ViewModel.Preference;

namespace ErogeHelper.View.MainGame
{
    public partial class MenuMainPage : Page
    {
        private readonly DoubleAnimation _fadeOutAnimation = AnimationTool.FadeOutAnimation;
        private readonly DoubleAnimation _fadeInAnimation = AnimationTool.FadeInAnimation;

        private readonly Subject<string> _pageSubject = new();
        public IObservable<string> PageChanged => _pageSubject;

        public MenuMainPage()
        {
            InitializeComponent();
            _fadeOutAnimation.Completed += (_, _) => SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            _fadeInAnimation.Completed += (_, _) => SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        public void FadeOut() => BeginAnimation(OpacityProperty, _fadeOutAnimation);

        public void FadeIn() => BeginAnimation(OpacityProperty, _fadeInAnimation);

        private void PreferenceOnClickEvent(object sender, EventArgs e) => DI.ShowView<PreferenceViewModel>();

        private void DeviceOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(PageTag.Device);
    }
}
