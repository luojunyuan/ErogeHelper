using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.MainGame
{
    public partial class MenuDevicePage : Page
    {
        public MenuDevicePage()
        {
            InitializeComponent();

            DeviceStoryboard = ApplyAnimation();
            // HACK: First time animation didn't play
            DeviceStoryboard.Begin();
            DeviceStoryboard.Stop();
        }

        public Storyboard DeviceStoryboard;
        //  begin position 
        public TranslateTransform volumeDownTransform = new(100, 0);
        public TranslateTransform backTransform = new(0, -100);

        private Storyboard ApplyAnimation()
        {
            var sb = new Storyboard();
            var volumeDownOpacityAnimation = CreateFadeOutAnimation();
            Storyboard.SetTarget(volumeDownOpacityAnimation, VolumeDown);
            Storyboard.SetTargetProperty(volumeDownOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(volumeDownOpacityAnimation);

            var volumeUpOpacityAnimation = CreateFadeOutAnimation();
            Storyboard.SetTarget(volumeUpOpacityAnimation, VolumeUp);
            Storyboard.SetTargetProperty(volumeUpOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(volumeUpOpacityAnimation);

            var backOpacityAnimation = CreateFadeOutAnimation();
            Storyboard.SetTarget(backOpacityAnimation, Back);
            Storyboard.SetTargetProperty(backOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(backOpacityAnimation);

            //VolumeDown
            VolumeDown.SetCurrentValue(RenderTransformProperty, volumeDownTransform);
            var volumeDownMoveAnimation = new DoubleAnimation()
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
            };
            Storyboard.SetTarget(volumeDownMoveAnimation, VolumeDown);
            Storyboard.SetTargetProperty(volumeDownMoveAnimation, new PropertyPath(XProperty));
            sb.Children.Add(volumeDownMoveAnimation);

            // Back
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            var backMoveAnimation = new DoubleAnimation()
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
            };
            Storyboard.SetTarget(backMoveAnimation, Back);
            Storyboard.SetTargetProperty(backMoveAnimation, new PropertyPath(YProperty));
            sb.Children.Add(backMoveAnimation);

            return sb;
        }

        private const string XProperty = "(UIElement.RenderTransform).(TranslateTransform.X)";
        private const string YProperty = "(UIElement.RenderTransform).(TranslateTransform.Y)";

        private static DoubleAnimation CreateFadeOutAnimation() => new()
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        };

        private async void Back_ClickEvent(object sender, EventArgs e)
        {
            DeviceStoryboard.AutoReverse = true;
            DeviceStoryboard.Begin();
            DeviceStoryboard.Pause();
            DeviceStoryboard.Seek(TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration));
            DeviceStoryboard.Resume();

            await Task.Delay((int)AssistiveTouch.TouchTransformDuration);

            NavigationService.GoBack();
        }
    }
}
