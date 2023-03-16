using ErogeHelper.AssistiveTouch.Helper;
using ErogeHelper.Share;
using ErogeHelper.Share.Languages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WindowsInput.Events;

namespace ErogeHelper.AssistiveTouch.Menu
{
    public partial class GamePage : Page, ITouchMenuPage
    {
        public event EventHandler<PageEventArgs>? PageChanged;

        public GamePage()
        {
            InitializeComponent();
            InitializeAnimation();

            var inFullscreen = Fullscreen.IsWindowFullscreen(App.GameWindowHandle);
            (FullScreenSwitcher.Symbol, FullScreenSwitcher.Text) = inFullscreen ?
                (Symbol.BackToWindow, Strings.AssistiveTouch_Window) :
                (Symbol.FullScreen, Strings.AssistiveTouch_FullScreen);

            TouchToMouse.Toggled += (_, _) =>
            {
                if (TouchToMouse.IsOn) TouchConversionHooker.Install();
                else TouchConversionHooker.UnInstall();
            };
        }

        public void Show(double moveDistance)
        {
            SetCurrentValue(VisibilityProperty, Visibility.Visible);

            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

            var fullscreenTransform = AnimationTool.RightOneTopOneTransform(moveDistance);
            var backTransform = AnimationTool.RightOneTransform(moveDistance);
            var closeGameTransform = AnimationTool.RightTwoTransform(moveDistance);

            FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, fullscreenTransform);
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            CloseGame.SetCurrentValue(RenderTransformProperty, closeGameTransform);

            _fullscreenMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.X);
            _fullscreenMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.Y);
            _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);
            _closeGameMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, closeGameTransform.X);

            _transitionInStoryboard.Begin();
        }

        public void Close()
        {
            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);
            _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
            _transitionInStoryboard.Begin();
            _transitionInStoryboard.Seek(TouchButton.MenuTransistDuration);
        }

        private readonly Storyboard _transitionInStoryboard = new();
        private readonly DoubleAnimation _fullscreenMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _fullscreenMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _closeGameMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

        private void InitializeAnimation()
        {
            AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

            AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveXAnimation, FullScreenSwitcher, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveYAnimation, FullScreenSwitcher, AnimationTool.YProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _closeGameMoveAnimation, CloseGame, AnimationTool.XProperty);

            _transitionInStoryboard.Completed += (_, _) =>
            {
                XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

                if (!_transitionInStoryboard.AutoReverse)
                {
                    FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                    Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                    CloseGame.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                }
                else
                {
                    _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                    SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    TouchMenuItem.ClickLocked = false;
                }
            };
        }

        private const int UIMinimumResponseTime = 50;
        private async void FullScreenSwitcherOnClickEvent(object sender, EventArgs e)
        {
            await WindowsInput.Simulate.Events()
                .Hold(KeyCode.Alt)
                .Hold(KeyCode.Enter)
                .Wait(UIMinimumResponseTime)
                .Release(KeyCode.Enter)
                .Release(KeyCode.Alt)
                .Invoke().ConfigureAwait(false);
        }

        private void BackOnClick(object sender, EventArgs e) => PageChanged?.Invoke(this, new(TouchMenuPageTag.GameBack));

        private void CloseGameOnClick(object sender, EventArgs e)
        {
            User32.PostMessage(
                App.GameProcess.MainWindowHandle,
                (uint)User32.WindowMessage.WM_SYSCOMMAND,
                (IntPtr)User32.SysCommand.SC_CLOSE);
            ((MainWindow)Application.Current.MainWindow).Menu.ManualClose();
        }
    }
}
