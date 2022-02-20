using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media.Animation;
using ErogeHelper.Platform.MISC;
using ErogeHelper.Shared.Contracts;
using WindowsInput.Events;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuDevicePage
{
    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuDevicePage()
    {
        InitializeComponent();
        ApplyTransitionInAnimation();
        if (!BrightnessAdjust.IsSupported)
        {
            BrightnessDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            BrightnessUp.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        }
    }

    public void TransitIn(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        GridPanel.Children.Cast<IMenuItemBackground>().Fill(false);

        var volumeDownTransform = AnimationTool.LeftOneTransform(moveDistance);
        var screenshotTransform = AnimationTool.RightOneTransform(moveDistance);
        var backTransform = AnimationTool.BottomOneTransform(moveDistance);
        var taskviewTransform = AnimationTool.LeftOneBottomOneTransform(moveDistance);
        var dockrightTransform = AnimationTool.RightOneBottomOneTransform(moveDistance);
        var brightnessDownTransform = AnimationTool.LeftOneBottomTwoTransform(moveDistance);
        var brightnessUpTransform = AnimationTool.BottomTwoTransform(moveDistance);
        VolumeDown.SetCurrentValue(RenderTransformProperty, volumeDownTransform);
        ScreenShot.SetCurrentValue(RenderTransformProperty, screenshotTransform);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        TaskView.SetCurrentValue(RenderTransformProperty, taskviewTransform);
        DockRight.SetCurrentValue(RenderTransformProperty, dockrightTransform);
        BrightnessDown.SetCurrentValue(RenderTransformProperty, brightnessDownTransform);
        BrightnessUp.SetCurrentValue(RenderTransformProperty, brightnessUpTransform);

        _volumeDownMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, volumeDownTransform.X);
        _screenshotMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, screenshotTransform.X);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.Y);
        _taskviewMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, taskviewTransform.X);
        _taskviewMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, taskviewTransform.Y);
        _dockrightMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, dockrightTransform.X);
        _dockrightMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, dockrightTransform.Y);
        _brightnessDownMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, brightnessDownTransform.X);
        _brightnessDownMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, brightnessDownTransform.Y);
        _brightnessUpMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, brightnessUpTransform.Y);

        _transitionInStoryboard.Begin();
    }

    public void TransitOut()
    {
        GridPanel.Children.Cast<IMenuItemBackground>().Fill(false);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration));
    }

    private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.DeviceBack);

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _volumeDownMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _screenshotMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _taskviewMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _taskviewMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _dockrightMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _dockrightMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _brightnessDownMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _brightnessDownMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _brightnessUpMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void ApplyTransitionInAnimation()
    {
        var pageOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(pageOpacityAnimation, this);
        Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty));
        _transitionInStoryboard.Children.Add(pageOpacityAnimation);


        Storyboard.SetTarget(_volumeDownMoveAnimation, VolumeDown);
        Storyboard.SetTargetProperty(_volumeDownMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_volumeDownMoveAnimation);

        Storyboard.SetTarget(_backMoveAnimation, Back);
        Storyboard.SetTargetProperty(_backMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_backMoveAnimation);

        Storyboard.SetTarget(_taskviewMoveXAnimation, TaskView);
        Storyboard.SetTargetProperty(_taskviewMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_taskviewMoveXAnimation);
        Storyboard.SetTarget(_taskviewMoveYAnimation, TaskView);
        Storyboard.SetTargetProperty(_taskviewMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_taskviewMoveYAnimation);

        Storyboard.SetTarget(_dockrightMoveXAnimation, DockRight);
        Storyboard.SetTargetProperty(_dockrightMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_dockrightMoveXAnimation);
        Storyboard.SetTarget(_dockrightMoveYAnimation, DockRight);
        Storyboard.SetTargetProperty(_dockrightMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_dockrightMoveYAnimation);

        Storyboard.SetTarget(_brightnessDownMoveXAnimation, BrightnessDown);
        Storyboard.SetTargetProperty(_brightnessDownMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_brightnessDownMoveXAnimation);
        Storyboard.SetTarget(_brightnessDownMoveYAnimation, BrightnessDown);
        Storyboard.SetTargetProperty(_brightnessDownMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_brightnessDownMoveYAnimation);

        Storyboard.SetTarget(_brightnessUpMoveYAnimation, BrightnessUp);
        Storyboard.SetTargetProperty(_brightnessUpMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_brightnessUpMoveYAnimation);

        Storyboard.SetTarget(_screenshotMoveXAnimation, ScreenShot);
        Storyboard.SetTargetProperty(_screenshotMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_screenshotMoveXAnimation);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            VolumeDown.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            ScreenShot.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            TaskView.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            DockRight.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            BrightnessDown.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            BrightnessUp.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            GridPanel.Children.Cast<IMenuItemBackground>().Fill(true);

            if (_transitionInStoryboard.AutoReverse)
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        };
    }

    private async void VolumeDownOnClickEvent(object sender, EventArgs e) =>
         await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeDown)
            .Invoke().ConfigureAwait(false);

    private async void VolumeUpOnClickEvent(object sender, EventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeUp)
            .Invoke().ConfigureAwait(false);

    private async void ActionCenterOnClickEvent(object sender, EventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.A)
            .Invoke().ConfigureAwait(false);

    private async void TaskViewOnClickEvent(object sender, EventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab)
            .Invoke().ConfigureAwait(false);

    private void BrightnessDownOnClickEvent(object sender, EventArgs e) => BrightnessAdjust.DecreaseBrightness();

    private void BrightnessUpOnClickEvent(object sender, EventArgs e) => BrightnessAdjust.IncreaseBrightness();

    private async void ScreenShotOnClickEvent(object sender, EventArgs e)
    {
        ((MainGameWindow)Application.Current.MainWindow).TouchMenu.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);

        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
            .Invoke().ConfigureAwait(false);

        await Task.Delay(ConstantValue.ScreenShotHideButtonTime).ConfigureAwait(true);

        ((MainGameWindow)Application.Current.MainWindow).SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }
}
