using System.Windows;
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

    public AssistiveTouchMenu()
    {
        InitializeComponent();
        ApplyTouchToMenuStoryboard();

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

        // Set animations' target value
        _widthAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _widthAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);
        _heightAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchSize);
        _heightAnimation.SetCurrentValue(DoubleAnimation.ToProperty, realWidth);

        // Show menu and begin animation
        SetCurrentValue(VisibilityProperty, Visibility.Visible);
        _touchToMenuStoryboard.Completed += (_, _) =>
        {
            SetCurrentValue(RenderTransformProperty, new TranslateTransform(0.0, 0.0));
            IsAnimating = false;
        };
        _touchToMenuStoryboard.Begin();
    }

    private readonly Storyboard _touchToMenuStoryboard = new() { FillBehavior = FillBehavior.Stop };
    private readonly DoubleAnimation _widthAnimation = AnimationTool.CreateSizeChangeAnimation();
    private readonly DoubleAnimation _heightAnimation = AnimationTool.CreateSizeChangeAnimation();

    /// <summary>
    /// Initialize menu x, y, width, height and frame animations.
    /// </summary>
    private void ApplyTouchToMenuStoryboard()
    {
        Storyboard.SetTarget(_widthAnimation, this);
        Storyboard.SetTargetProperty(_widthAnimation, new PropertyPath(WidthProperty));
        _touchToMenuStoryboard.Children.Add(_widthAnimation);
        Storyboard.SetTarget(_heightAnimation, this);
        Storyboard.SetTargetProperty(_heightAnimation, new PropertyPath(HeightProperty));
        _touchToMenuStoryboard.Children.Add(_heightAnimation);

        var menuXMoveAnimation = AnimationTool.CreateTransformMoveToZeroAnimation();
        Storyboard.SetTarget(menuXMoveAnimation, this);
        Storyboard.SetTargetProperty(menuXMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _touchToMenuStoryboard.Children.Add(menuXMoveAnimation);
        var menuYMoveAnimation = AnimationTool.CreateTransformMoveToZeroAnimation();
        Storyboard.SetTarget(menuYMoveAnimation, this);
        Storyboard.SetTargetProperty(menuYMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _touchToMenuStoryboard.Children.Add(menuYMoveAnimation);

        var frameOpacityAnimation = AnimationTool.CreateFadeInAnimation();
        Storyboard.SetTarget(frameOpacityAnimation, MenuContent);
        Storyboard.SetTargetProperty(frameOpacityAnimation, new PropertyPath(OpacityProperty));
        _touchToMenuStoryboard.Children.Add(frameOpacityAnimation);
    }

    public void Hide()
    {
        IsOpen = false;
        IsAnimating = true;

        SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        Closed?.Invoke(this, new());

        IsAnimating = false;
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
