using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.View.MainGame.AssistiveMenu;

namespace ErogeHelper.View.MainGame
{
    public partial class MenuDevicePage : Page
    {
        public MenuDevicePage()
        {
            InitializeComponent();
            ApplyTransistionInAnimation();
        }

        private readonly Subject<string> _pageSubject = new();
        public IObservable<string> PageChanged => _pageSubject;

        public readonly Storyboard TransitionInStoryboard = new();
        private readonly TranslateTransform volumeDownTransform = new(100, 0);
        private readonly TranslateTransform backTransform = new(0, -100);

        private void ApplyTransistionInAnimation()
        {
            var pageOpacityAnimation = AnimationTool.FadeInAnimation;
            Storyboard.SetTarget(pageOpacityAnimation, this);
            Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty));
            TransitionInStoryboard.Children.Add(pageOpacityAnimation);

            //VolumeDown
            VolumeDown.SetCurrentValue(RenderTransformProperty, volumeDownTransform);
            var volumeDownMoveAnimation = new DoubleAnimation()
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
            };
            Storyboard.SetTarget(volumeDownMoveAnimation, VolumeDown);
            Storyboard.SetTargetProperty(volumeDownMoveAnimation, new PropertyPath(AnimationTool.XProperty));
            TransitionInStoryboard.Children.Add(volumeDownMoveAnimation);

            // Back
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            var backMoveAnimation = new DoubleAnimation()
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
            };
            Storyboard.SetTarget(backMoveAnimation, Back);
            Storyboard.SetTargetProperty(backMoveAnimation, new PropertyPath(AnimationTool.YProperty));
            TransitionInStoryboard.Children.Add(backMoveAnimation);
        }

        private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(PageTag.DeviceBack);
    }
}
