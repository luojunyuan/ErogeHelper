using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media.Animation;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.MainGame.AssistiveTouchMenu;
using ModernWpf.Controls;
using ReactiveUI;
using WindowsInput.Events;
using Page = System.Windows.Controls.Page;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuDevicePage : Page, IViewFor<MenuDeviceViewModel>
{
    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(MenuDeviceViewModel),
        typeof(MenuDevicePage));

    public MenuDeviceViewModel? ViewModel
    {
        get => (MenuDeviceViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (MenuDeviceViewModel?)value);
    }
    #endregion

    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuDevicePage()
    {
        InitializeComponent();
        ApplyTransistionInAnimation();

        ViewModel = DependencyResolver.GetService<MenuDeviceViewModel>();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
              vm => vm.SwitchFullScreenIcon,
              v => v.FullScreenSwitcher.Symbol,
              iconEnum => iconEnum switch
              {
                  SymbolName.BackToWindow => Symbol.BackToWindow,
                  SymbolName.FullScreen => Symbol.FullScreen,
                  _ => throw new InvalidCastException(),
              }).DisposeWith(d);
            this.OneWayBind(ViewModel,
              vm => vm.SwitchFullScreenText,
              v => v.FullScreenSwitcher.Text).DisposeWith(d);
        });
    }

    public void TransistIn(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        GridPanel.Children.Cast<MenuItemControl>().FillBackground(false);

        var volumeDownTransform = AnimationTool.LeftOneTransform(moveDistance);
        var fullscreenTransform = AnimationTool.RightOneTransform(moveDistance);
        var backTransform = AnimationTool.BottomOneTransform(moveDistance);
        var taskviewTransform = AnimationTool.BottomOneLeftOneTransform(moveDistance);
        var dockrightTransform = AnimationTool.BottomOneRightOneTransform(moveDistance);
        var screenshotTransform = AnimationTool.BottomTwoRightOneTransform(moveDistance);
        VolumeDown.SetCurrentValue(RenderTransformProperty, volumeDownTransform);
        FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, fullscreenTransform);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        TaskView.SetCurrentValue(RenderTransformProperty, taskviewTransform);
        DockRight.SetCurrentValue(RenderTransformProperty, dockrightTransform);
        ScreenShot.SetCurrentValue(RenderTransformProperty, screenshotTransform);

        _volumeDownMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, volumeDownTransform.X);
        _fullscreenMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.X);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.Y);
        _taskviewMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, taskviewTransform.X);
        _taskviewMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, taskviewTransform.Y);
        _dockrightMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, dockrightTransform.X);
        _dockrightMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, dockrightTransform.Y);
        _screenshotMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, screenshotTransform.X);
        _screenshotMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, screenshotTransform.Y);

        _transitionInStoryboard.Begin();
    }

    public void TransistOut()
    {
        GridPanel.Children.Cast<MenuItemControl>().FillBackground(false);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration));
    }

    private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.DeviceBack);

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _volumeDownMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _fullscreenMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _taskviewMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _taskviewMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _dockrightMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _dockrightMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _screenshotMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _screenshotMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void ApplyTransistionInAnimation()
    {
        var pageOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(pageOpacityAnimation, this);
        Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty));
        _transitionInStoryboard.Children.Add(pageOpacityAnimation);


        Storyboard.SetTarget(_volumeDownMoveAnimation, VolumeDown);
        Storyboard.SetTargetProperty(_volumeDownMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_volumeDownMoveAnimation);

        Storyboard.SetTarget(_fullscreenMoveAnimation, FullScreenSwitcher);
        Storyboard.SetTargetProperty(_fullscreenMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_fullscreenMoveAnimation);

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

        Storyboard.SetTarget(_screenshotMoveXAnimation, ScreenShot);
        Storyboard.SetTargetProperty(_screenshotMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_screenshotMoveXAnimation);
        Storyboard.SetTarget(_screenshotMoveYAnimation, ScreenShot);
        Storyboard.SetTargetProperty(_screenshotMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_screenshotMoveYAnimation);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            VolumeDown.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            TaskView.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            DockRight.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            ScreenShot.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            GridPanel.Children.Cast<MenuItemControl>().FillBackground(true);

            if (_transitionInStoryboard.AutoReverse == true)
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

    private async void FullScreenSwitcherOnClickEvent(object sender, EventArgs e)
    {
        // アインシュタイン not work
        ViewModel!.SwitchFullScreen.Execute(Unit.Default).Subscribe();
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.Alt, KeyCode.Enter)
            .Invoke().ConfigureAwait(false);
    }

    private async void ActionCenterOnClickEvent(object sender, EventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.A)
            .Invoke().ConfigureAwait(false);

    private async void TaskViewOnClickEvent(object sender, EventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab)
            .Invoke().ConfigureAwait(false);

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
