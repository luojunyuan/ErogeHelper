using System.Reactive;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ErogeHelper.View.MainGame.AssistiveMenu;
using Splat;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveTouchMenu : IEnableLogger
{
    // TODO: Consider bigger menu 500pixel
    private const double MaxSizeOfMenu = 300;
    private const int EndureEdgeHeight = 30;

    public event EventHandler? Closed;
    public Action ShowTouchCallback = null!;

    public AssistiveTouchMenu()
    {
        InitializeComponent();
        ApplyTouchToMenuStoryboard();
        ApplyMenuToTouchStoryboard();

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
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(13.75));
        var relativePos = touchPos - middlePoint + new Point(touchSize / 2, touchSize / 2);
        SetCurrentValue(RenderTransformProperty, new TranslateTransform(relativePos.X, relativePos.Y));
        FackWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // Set animations' target value
        _widthShowAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _widthShowAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);
        _heightShowAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _heightShowAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);

        // Show menu and begin animation
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // FIXME: Position is wrong when animating first time 
        _touchToMenuStoryboard.Begin();
    }

    private Point _touchPosWhenHideAnimationComplete;

    public void Hide(Point middlePoint, Point touchPos, double touchSize)
    {
        _touchPosWhenHideAnimationComplete = touchPos - middlePoint + new Point(touchSize / 2, touchSize / 2);
        FackWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Visible);

        // Set animations' target value
        _widthHideAnimation.SetCurrentValue(DoubleAnimation.FromProperty, Width);
        _widthHideAnimation.SetCurrentValue(DoubleAnimation.ToProperty, 60.0);
        _heightHideAnimation.SetCurrentValue(DoubleAnimation.FromProperty, Width);
        _heightHideAnimation.SetCurrentValue(DoubleAnimation.ToProperty, 60.0);
        _menuXMoveAnimation.SetCurrentValue(DoubleAnimation.ToProperty, _touchPosWhenHideAnimationComplete.X);
        _menuYMoveAnimation.SetCurrentValue(DoubleAnimation.ToProperty, _touchPosWhenHideAnimationComplete.Y);

        _menuToTouchStoryboard.Begin();
    }

    private void ResizeMenu(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged && e.NewSize.Height > 30)
        {
            if (e.NewSize.Height > EndureEdgeHeight + MaxSizeOfMenu)
            {
                SetCurrentValue(HeightProperty, MaxSizeOfMenu);
                SetCurrentValue(WidthProperty, MaxSizeOfMenu);
            }
            else
            {
                SetCurrentValue(HeightProperty, e.NewSize.Height - 30);
                SetCurrentValue(WidthProperty, e.NewSize.Height - 30);
            }
        }
    }

    private readonly Storyboard _touchToMenuStoryboard = new() { FillBehavior = FillBehavior.Stop };
    private readonly DoubleAnimation _widthShowAnimation = AnimationTool.SizeChangeAnimation;
    private readonly DoubleAnimation _heightShowAnimation = AnimationTool.SizeChangeAnimation;

    private readonly Storyboard _menuToTouchStoryboard = new() { FillBehavior = FillBehavior.Stop };
    private readonly DoubleAnimation _widthHideAnimation = AnimationTool.SizeChangeAnimation;
    private readonly DoubleAnimation _heightHideAnimation = AnimationTool.SizeChangeAnimation;
    private DoubleAnimation _menuXMoveAnimation = AnimationTool.TransformMoveToTargetAnimation;
    private DoubleAnimation _menuYMoveAnimation = AnimationTool.TransformMoveToTargetAnimation;

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

        var frameOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(frameOpacityAnimation, MenuContent);
        Storyboard.SetTargetProperty(frameOpacityAnimation, new PropertyPath(OpacityProperty));
        _touchToMenuStoryboard.Children.Add(frameOpacityAnimation);

        var fackWhitePointOpacityAnimation = AnimationTool.FadeOutAnimation;
        Storyboard.SetTarget(fackWhitePointOpacityAnimation, FackWhitePoint);
        Storyboard.SetTargetProperty(fackWhitePointOpacityAnimation, new PropertyPath(OpacityProperty));
        _touchToMenuStoryboard.Children.Add(fackWhitePointOpacityAnimation);

        _touchToMenuStoryboard.Completed += (_, _) =>
        {
            SetCurrentValue(RenderTransformProperty, new TranslateTransform(0.0, 0.0));
            FackWhitePoint.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
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

        var frameOpacityAnimation = AnimationTool.FadeOutAnimation;
        Storyboard.SetTarget(frameOpacityAnimation, MenuContent);
        Storyboard.SetTargetProperty(frameOpacityAnimation, new PropertyPath(OpacityProperty));
        _menuToTouchStoryboard.Children.Add(frameOpacityAnimation);

        var fackWhitePointOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(fackWhitePointOpacityAnimation, FackWhitePoint);
        Storyboard.SetTargetProperty(fackWhitePointOpacityAnimation, new PropertyPath(OpacityProperty));
        _menuToTouchStoryboard.Children.Add(fackWhitePointOpacityAnimation);

        _menuToTouchStoryboard.Completed += (_, _) =>
        {
            ShowTouchCallback();
            SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            Closed?.Invoke(this, new());
            IsAnimating = IsOpen = false;
        };
    }



    // https://paulstovell.com/wpf-navigation/
    private void MenuContentOnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        if (e.Content is MenuMainPage menuMainPage)
        {
        }
        else if (e.Content is MenuDevicePage menuDevicePage)
        {
            //var sb = e.ExtraData as Storyboard;
            //sb.Begin();
            menuDevicePage.DeviceStoryboard.Begin();
            // back to the right position
            //menuDevicePage.volumeDownTransform.SetCurrentValue(System.Windows.Media.TranslateTransform.XProperty, (double)0);
            // 开始动画后，设置各个控件TranslateTransform的移动位置
            this.Log().Debug("menuDevicePage");
        }
        else
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
            }
        }
        //var ta = new ThicknessAnimation();
        //ta.Duration = TimeSpan.FromSeconds(0.3);
        //ta.DecelerationRatio = 0.7;
        //ta.To = new Thickness(0, 0, 0, 0);
        //if (e.NavigationMode == NavigationMode.New)
        //{
        //    ta.From = new Thickness(500, 0, 0, 0);
        //}
        //else if (e.NavigationMode == NavigationMode.Back)
        //{
        //    ta.From = new Thickness(0, 0, 500, 0);
        //}
        //(e.Content as Page).BeginAnimation(MarginProperty, ta);
    }
}
