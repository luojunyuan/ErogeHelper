using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.View.MainGame.AssistiveMenu;
using Splat;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveTouchMenu : IEnableLogger
{
    public event EventHandler? Closed;
    public Action ShowTouchCallback = null!;

    private readonly MenuMainPage _menuMainPage = new();
    private readonly MenuDevicePage _menuDevicePage = new();

    public AssistiveTouchMenu()
    {
        InitializeComponent();
        MainMenu.Navigate(_menuMainPage);
        DeviceMenu.Navigate(_menuDevicePage);
        ApplyTouchToMenuStoryboard();
        ApplyMenuToTouchStoryboard();

        _menuMainPage.PageChanged
            .Merge(_menuDevicePage.PageChanged)
            .Subscribe(PageNavigation);

        Loaded += (_, _) =>
        {
            var parent = Parent as FrameworkElement
                ?? throw new InvalidOperationException("Control's parent must be FrameworkElement type");

            parent.SizeChanged += ResizeMenu;
        };
    }
    public bool IsOpen { get; private set; }

    public bool IsAnimating { get; private set; }

    public void Show(Point middlePoint, Point touchPos, double touchSize)
    {
        IsAnimating = IsOpen = true;

        var realWidth = Width;

        // Initilize values
        SetCurrentValue(HeightProperty, touchSize);
        SetCurrentValue(WidthProperty, touchSize);
        SetCurrentValue(PaddingProperty, new Thickness());
        //SetCurrentValue(CornerRadiusProperty, new CornerRadius(13.75));
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(25));
        var relativePos = touchPos - middlePoint + new Point(touchSize / 2, touchSize / 2);
        SetCurrentValue(RenderTransformProperty, new TranslateTransform(relativePos.X, relativePos.Y));
        FakeWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // Set animations' target value
        _widthShowAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _widthShowAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);
        _heightShowAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _heightShowAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);

        // Show menu and begin animation
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // FIXME: Multi-Screen issue: Position is wrong when animating for the first time
        _touchToMenuStoryboard.Begin();
    }

    private Point _touchPosWhenHideAnimationComplete;

    public void Hide(Point middlePoint, Point touchPos, double touchSize)
    {
        _touchPosWhenHideAnimationComplete = touchPos - middlePoint + new Point(touchSize / 2, touchSize / 2);
        FakeWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // Set animations' target value
        _widthHideAnimation.SetCurrentValue(DoubleAnimation.FromProperty, Width);
        _widthHideAnimation.SetCurrentValue(DoubleAnimation.ToProperty, touchSize);
        _heightHideAnimation.SetCurrentValue(DoubleAnimation.FromProperty, Width);
        _heightHideAnimation.SetCurrentValue(DoubleAnimation.ToProperty, touchSize);
        _menuXMoveAnimation.SetCurrentValue(DoubleAnimation.ToProperty, _touchPosWhenHideAnimationComplete.X);
        _menuYMoveAnimation.SetCurrentValue(DoubleAnimation.ToProperty, _touchPosWhenHideAnimationComplete.Y);

        _menuToTouchStoryboard.Begin();
    }

    public void UpdateMenuStatus(double windowHeight)
    {
        EndureEdgeHeight = MaxHeight / 10;
        UpdateMenuSize(windowHeight);
    }

    private double EndureEdgeHeight;

    private void ResizeMenu(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged && e.NewSize.Height > EndureEdgeHeight)
        {
            UpdateMenuSize(e.NewSize.Height);
        }
    }

    private void UpdateMenuSize(double newGameWindowHeight)
    {
        if (newGameWindowHeight > EndureEdgeHeight + MaxHeight)
        {
            Height = MaxHeight;
            Width = MaxHeight;
        }
        else
        {
            Height = newGameWindowHeight - EndureEdgeHeight;
            Width = newGameWindowHeight - EndureEdgeHeight;
        }
    }

    private void PageNavigation(string nav)
    {
        switch (nav)
        {
            case PageTag.Device:
                _menuMainPage.FadeOut();
                _menuDevicePage.TransistIn();
                break;
            case PageTag.DeviceBack:
                _menuMainPage.FadeIn();
                _menuDevicePage.TransistOut();
                break;
            default:
                break;
        }
    }

    private readonly Storyboard _touchToMenuStoryboard = new() { FillBehavior = FillBehavior.Stop };
    private readonly DoubleAnimation _widthShowAnimation = AnimationTool.SizeChangeAnimation;
    private readonly DoubleAnimation _heightShowAnimation = AnimationTool.SizeChangeAnimation;

    private readonly Storyboard _menuToTouchStoryboard = new() { FillBehavior = FillBehavior.Stop };
    private readonly DoubleAnimation _widthHideAnimation = AnimationTool.SizeChangeAnimation;
    private readonly DoubleAnimation _heightHideAnimation = AnimationTool.SizeChangeAnimation;
    private readonly DoubleAnimation _menuXMoveAnimation = AnimationTool.TransformMoveToTargetAnimation;
    private readonly DoubleAnimation _menuYMoveAnimation = AnimationTool.TransformMoveToTargetAnimation;

    private void ApplyTouchToMenuStoryboard()
    {
        Storyboard.SetTarget(_widthShowAnimation, this);
        Storyboard.SetTargetProperty(_widthShowAnimation, new PropertyPath(WidthProperty));
        _touchToMenuStoryboard.Children.Add(_widthShowAnimation);
        Storyboard.SetTarget(_heightShowAnimation, this);
        Storyboard.SetTargetProperty(_heightShowAnimation, new PropertyPath(HeightProperty));
        _touchToMenuStoryboard.Children.Add(_heightShowAnimation);

        var menuXMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
        Storyboard.SetTarget(menuXMoveAnimation, this);
        Storyboard.SetTargetProperty(menuXMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _touchToMenuStoryboard.Children.Add(menuXMoveAnimation);
        var menuYMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
        Storyboard.SetTarget(menuYMoveAnimation, this);
        Storyboard.SetTargetProperty(menuYMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _touchToMenuStoryboard.Children.Add(menuYMoveAnimation);

        var paddingAnimation = new ThicknessAnimation()
        {
            From = new(0),
            To = new(12),
            Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        };
        Storyboard.SetTarget(paddingAnimation, this);
        Storyboard.SetTargetProperty(paddingAnimation, new PropertyPath(PaddingProperty));
        _touchToMenuStoryboard.Children.Add(paddingAnimation);

        var frameBaseOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(frameBaseOpacityAnimation, FrameBase);
        Storyboard.SetTargetProperty(frameBaseOpacityAnimation, new PropertyPath(OpacityProperty));
        _touchToMenuStoryboard.Children.Add(frameBaseOpacityAnimation);

        var fakeWhitePointOpacityAnimation = AnimationTool.FadeOutAnimation;
        Storyboard.SetTarget(fakeWhitePointOpacityAnimation, FakeWhitePoint);
        Storyboard.SetTargetProperty(fakeWhitePointOpacityAnimation, new PropertyPath(OpacityProperty));
        _touchToMenuStoryboard.Children.Add(fakeWhitePointOpacityAnimation);

        _touchToMenuStoryboard.Completed += (_, _) =>
        {
            SetCurrentValue(RenderTransformProperty, new TranslateTransform(0.0, 0.0));
            FakeWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            SetCurrentValue(PaddingProperty, new Thickness(12));
            IsAnimating = false;
        };
    }

    private void ApplyMenuToTouchStoryboard()
    {
        Storyboard.SetTarget(_widthHideAnimation, this);
        Storyboard.SetTargetProperty(_widthHideAnimation, new PropertyPath(WidthProperty));
        _menuToTouchStoryboard.Children.Add(_widthHideAnimation);
        Storyboard.SetTarget(_heightHideAnimation, this);
        Storyboard.SetTargetProperty(_heightHideAnimation, new PropertyPath(HeightProperty));
        _menuToTouchStoryboard.Children.Add(_heightHideAnimation);

        Storyboard.SetTarget(_menuXMoveAnimation, this);
        Storyboard.SetTargetProperty(_menuXMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _menuToTouchStoryboard.Children.Add(_menuXMoveAnimation);
        Storyboard.SetTarget(_menuYMoveAnimation, this);
        Storyboard.SetTargetProperty(_menuYMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _menuToTouchStoryboard.Children.Add(_menuYMoveAnimation);

        var frameBaseOpacityAnimation = AnimationTool.FadeOutAnimation;
        Storyboard.SetTarget(frameBaseOpacityAnimation, FrameBase);
        Storyboard.SetTargetProperty(frameBaseOpacityAnimation, new PropertyPath(OpacityProperty));
        _menuToTouchStoryboard.Children.Add(frameBaseOpacityAnimation);

        var paddingAnimation = new ThicknessAnimation()
        {
            From = new(12),
            To = new(0),
            Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        };
        Storyboard.SetTarget(paddingAnimation, this);
        Storyboard.SetTargetProperty(paddingAnimation, new PropertyPath(PaddingProperty));
        _menuToTouchStoryboard.Children.Add(paddingAnimation);

        var fackWhitePointOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(fackWhitePointOpacityAnimation, FakeWhitePoint);
        Storyboard.SetTargetProperty(fackWhitePointOpacityAnimation, new PropertyPath(OpacityProperty));
        _menuToTouchStoryboard.Children.Add(fackWhitePointOpacityAnimation);

        _menuToTouchStoryboard.Completed += (_, _) =>
        {
            ShowTouchCallback();
            SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            SetCurrentValue(PaddingProperty, new Thickness(0));
            Closed?.Invoke(this, new());
            IsAnimating = IsOpen = false;

            _menuMainPage.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            _menuDevicePage.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        };
    }
}
