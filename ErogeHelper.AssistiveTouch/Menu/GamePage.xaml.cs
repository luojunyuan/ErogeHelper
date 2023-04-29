using ErogeHelper.AssistiveTouch.Core;
using ErogeHelper.AssistiveTouch.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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

            VirtualKeyboard.Toggled += (_, _) =>
            {
                if (VirtualKeyboard.IsOn)
                {
                }
                else
                {
                }
            };

            void SetFullscreenSwitcher(bool inFullscreen) =>
                (FullScreenSwitcher.Symbol, FullScreenSwitcher.Text) = inFullscreen ?
                    (Symbol.BackToWindow, XamlResource.GetString("AssistiveTouch_Window")) :
                    (Symbol.Fullscreen, XamlResource.GetString("AssistiveTouch_Fullscreen"));
            SetFullscreenSwitcher(Fullscreen.IsWindowFullscreen(App.GameWindowHandle));
            Fullscreen.FullscreenChanged += (_, isFullscreen) => SetFullscreenSwitcher(isFullscreen);

            TouchToMouse.Toggled += (_, _) =>
            {
                if (TouchToMouse.IsOn) TouchConversionHooker.Install();
                else TouchConversionHooker.UnInstall();
            };

            // For second inside menu
            _fadeOutAnimation.Completed += (_, _) =>
            {
                Visibility = Visibility.Hidden;
                TouchMenuItem.ClickLocked = false;
            };
            _fadeOutAnimation.Freeze();
        }

        public void Show(double moveDistance)
        {
            SetCurrentValue(VisibilityProperty, Visibility.Visible);

            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

            var keyboardTransform = AnimationTool.TopOneTransform(moveDistance);
            var fullscreenTransform = AnimationTool.RightOneTopOneTransform(moveDistance);
            var moveGameTransform = AnimationTool.RightTwoTopOneTransform(moveDistance);
            var backTransform = AnimationTool.RightOneTransform(moveDistance);
            var closeGameTransform = AnimationTool.RightTwoTransform(moveDistance);

            VirtualKeyboard.SetCurrentValue(RenderTransformProperty, keyboardTransform);
            FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, fullscreenTransform);
            MoveGame.SetCurrentValue(RenderTransformProperty, moveGameTransform);
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            CloseGame.SetCurrentValue(RenderTransformProperty, closeGameTransform);

            _keyboardAnimation.SetCurrentValue(DoubleAnimation.FromProperty, keyboardTransform.Y);
            _fullscreenMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.X);
            _fullscreenMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.Y);
            _moveGameMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, moveGameTransform.X);
            _moveGameMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, moveGameTransform.Y);
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
        private readonly DoubleAnimation _keyboardAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _fullscreenMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _fullscreenMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _moveGameMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _moveGameMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
        private readonly DoubleAnimation _closeGameMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

        private void InitializeAnimation()
        {
            AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

            AnimationTool.BindingAnimation(_transitionInStoryboard, _keyboardAnimation, VirtualKeyboard, AnimationTool.YProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveXAnimation, FullScreenSwitcher, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveYAnimation, FullScreenSwitcher, AnimationTool.YProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _moveGameMoveXAnimation, MoveGame, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _moveGameMoveYAnimation, MoveGame, AnimationTool.YProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(_transitionInStoryboard, _closeGameMoveAnimation, CloseGame, AnimationTool.XProperty);

            _transitionInStoryboard.Completed += (_, _) =>
            {
                XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

                if (!_transitionInStoryboard.AutoReverse)
                {
                    VirtualKeyboard.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                    FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                    MoveGame.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
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
            HwndTools.WindowLostFocus(MainWindow.Handle, true);
            await WindowsInput.Simulate.Events()
                .Hold(KeyCode.Alt)
                .Hold(KeyCode.Enter)
                .Wait(UIMinimumResponseTime)
                .Release(KeyCode.Enter)
                .Release(KeyCode.Alt)
                .Invoke().ConfigureAwait(false);
            HwndTools.WindowLostFocus(MainWindow.Handle, false);
        }

        private readonly DoubleAnimation _fadeOutAnimation = AnimationTool.FadeOutAnimation;
        private void CloseInside() => BeginAnimation(OpacityProperty, _fadeOutAnimation);

        private void MoveGameOnClick(object sender, EventArgs e)
        {
            CloseInside();
            PageChanged?.Invoke(this, new(TouchMenuPageTag.WinMove));
        }

        private void BackOnClick(object sender, EventArgs e) => PageChanged?.Invoke(this, new(TouchMenuPageTag.GameBack));

        private const int MenuTransistDuration = 200;
        private async void CloseGameOnClick(object sender, EventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Menu.ManualClose();
            await WindowsInput.Simulate.Events()
                .Wait(MenuTransistDuration)
                .ClickChord(KeyCode.Alt, KeyCode.F4)
                .Invoke().ConfigureAwait(false);
        }
    }
}
