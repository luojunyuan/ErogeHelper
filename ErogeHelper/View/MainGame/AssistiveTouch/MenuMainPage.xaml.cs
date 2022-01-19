using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Platform;
using ErogeHelper.ViewModel.Preference;

namespace ErogeHelper.View.MainGame
{
    public partial class MenuMainPage : Page
    {
        public readonly Storyboard _devicePageStoryboard;

        public MenuMainPage()
        {
            InitializeComponent();

            _devicePageStoryboard = ApplyAnimation();
        }

        private void PreferenceOnClickEvent(object sender, EventArgs e) => DI.ShowView<PreferenceViewModel>();

        private async void DeviceOnClickEvent(object sender, EventArgs e)
        {
            _devicePageStoryboard.Begin();

            await Task.Delay((int)AssistiveTouch.TouchTransformDuration);
            //NavigationService.Navigate(new Uri("MenuDevicePage.xaml", UriKind.Relative));
            NavigationService.Navigate(new MenuDevicePage());
        }


        private Storyboard ApplyAnimation()
        {
            var sb = new Storyboard();
            var deviceOpacityAnimation = CreateFadeInAnimation();
            Storyboard.SetTarget(deviceOpacityAnimation, Device);
            Storyboard.SetTargetProperty(deviceOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(deviceOpacityAnimation);

            var functionOpacityAnimation = CreateFadeInAnimation();
            Storyboard.SetTarget(functionOpacityAnimation, Function);
            Storyboard.SetTargetProperty(functionOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(functionOpacityAnimation);

            var preferenceOpacityAnimation = CreateFadeInAnimation();
            Storyboard.SetTarget(preferenceOpacityAnimation, Preference);
            Storyboard.SetTargetProperty(preferenceOpacityAnimation, new PropertyPath(OpacityProperty));
            sb.Children.Add(preferenceOpacityAnimation);

            return sb;
        }

        private static DoubleAnimation CreateFadeInAnimation() => new()
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        };

        private void AnimationReverse()
        {
            //TranslateTransform transform = new TranslateTransform(0.0, 0.0);
            //myBox.RenderTransform = transform;

            //sb = new Storyboard();
            //Duration dur = new Duration(TimeSpan.FromSeconds(0.5));
            //DoubleAnimation shiftAnimation = new DoubleAnimation(100.0, dur);
            //shiftAnimation.AutoReverse = true;
            //sb.Children.Add(shiftAnimation);
            //Storyboard.SetTarget(shiftAnimation, myBox);
            //Storyboard.SetTargetProperty(shiftAnimation, new PropertyPath("RenderTransform.X"));

            //sb.Begin();
            //sb.Pause();
            //sb.Seek(sb.Duration.TimeSpan);
            //sb.Resume();
        }


    }
}
