using ErogeHelper.AssistiveTouch.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WindowsInput.Events;

namespace ErogeHelper.AssistiveTouch.Menu
{
    public partial class FunctionPage : Page, ITouchMenuPage
    {
        public event EventHandler<PageEventArgs>? PageChanged;

        public FunctionPage()
        {
            InitializeComponent();
            InitializeAnimation();
        }

        public void Show(double moveDistance)
        {
            SetCurrentValue(VisibilityProperty, Visibility.Visible);

            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

            var backTransform = AnimationTool.LeftOneTransform(moveDistance);
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);

            _transitionInStoryboard.Begin();
        }

        public void Close()
        {
            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);
            _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
            _transitionInStoryboard.Begin();
            _transitionInStoryboard.Seek(TouchButton.MenuTransistDuration);
        }

        private void BackOnClickEvent(object sender, EventArgs e) => PageChanged?.Invoke(this, new(TouchMenuPageTag.FunctionBack));

        private readonly Storyboard _transitionInStoryboard = new();
        private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

        private void InitializeAnimation()
        {
            AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

            AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);

            _transitionInStoryboard.Completed += (_, _) =>
            {
                XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

                if (!_transitionInStoryboard.AutoReverse)
                {
                    Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                }
                else
                {
                    _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                    SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    TouchMenuItem.ClickLocked = false;
                }
            };
        }
        private void BatteryOnClickEvent(object sender, EventArgs e)
        {
        }
    }
}
