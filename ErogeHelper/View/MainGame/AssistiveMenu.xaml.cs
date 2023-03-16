using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Function.WpfComponent;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.View.MainGame.Menu;
using ReactiveMarbles.ObservableEvents;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveMenu : Border
{
    public static TimeSpan MenuTransistDuration { get; } = TimeSpan.FromMilliseconds(200);

    private readonly ITouchMenuPage _menuMainPage = new MainPage();
    private readonly ITouchMenuPage _menuDevicePage = new DevicePage();
    private readonly ITouchMenuPage _menuGamePage = new GamePage();
    private readonly ITouchMenuPage _menuFunctionPage = new FunctionPage();

    public AssistiveMenu()
    {
        InitializeComponent();
        XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

        MainMenu.Navigate(_menuMainPage);
        GameMenu.Navigate(_menuGamePage);
        DeviceMenu.Navigate(_menuDevicePage);
        FunctionMenu.Navigate(_menuFunctionPage);

        _menuMainPage.PageChanged
            .Merge(_menuGamePage.PageChanged)
            .Merge(_menuDevicePage.PageChanged)
            .Merge(_menuFunctionPage.PageChanged)
            .Subscribe(PageNavigation);

        var mainWindow = (MainGameWindow)Application.Current.MainWindow;

        var firstTimeMenuInit = this.Events().Loaded
            .Select(_ => mainWindow.Touch.ViewModel.BigTouchChanged.Value);
        var bigChanged = mainWindow.Touch.ViewModel.BigTouchChanged.Skip(1);
        firstTimeMenuInit
            .Merge(bigChanged)
            .Select(big => big ? 100.0 : 60)
            .Subscribe(UpdateProperties);

        mainWindow.Events().SizeChanged
            .SkipUntil(this.Events().Loaded)
            .Where(e => e.HeightChanged && e.NewSize.Height > EndureEdgeHeight)
            .Select(e => e.NewSize.Height)
            .Subscribe(UpdateMenuSize);

        #region Move Logic
        var isOpen = false;
        var isAnimating = false;

        mainWindow.Touch.Clicked.Subscribe(_ =>
        {
            TouchMenuItem.ClickLocked = true;
            Visibility = Visibility.Visible;
            mainWindow.Touch.Visibility = Visibility.Hidden;
            mainWindow.Touch.IsTouchMenuOpend = isOpen = isAnimating = true;

            _menuMainPage.Visibility = Visibility.Visible;
            _menuGamePage.Visibility = Visibility.Collapsed;
            _menuDevicePage.Visibility = Visibility.Collapsed;
            _menuFunctionPage.Visibility = Visibility.Collapsed;

            var relativeBeginX = mainWindow.Width / 2 - mainWindow.Touch.ActualWidth / 2;
            var relativeBeginY = mainWindow.Height / 2 - mainWindow.Touch.ActualWidth / 2;

            TransformXAnimation.From = mainWindow.Touch.TouchPosTransform.X - relativeBeginX;
            TransformYAnimation.From = mainWindow.Touch.TouchPosTransform.Y - relativeBeginY;
            MovementStoryboard.Begin();
        });

        mainWindow.Events().Deactivated
                .Where(_ => isOpen == true)
            .Merge(this.Events().PreviewMouseLeftButtonUp
                .Where(e => e.OriginalSource is AssistiveMenu && !TouchMenuItem.ClickLocked))
            .Where(_ => !isAnimating)
            .Subscribe(_ =>
            {
                isAnimating = true;
                FakeWhitePoint.Visibility = Visibility.Visible;
                MovementStoryboard.AutoReverse = true;
                MovementStoryboard.Begin();
                MovementStoryboard.Seek(MenuTransistDuration);
            });

        // Mene opend
        MovementStoryboard.Events().Completed
           .Where(_ => MovementStoryboard.AutoReverse == false)
           .Subscribe(_ =>
           {
               isAnimating = false;
               TouchMenuItem.ClickLocked = false;
               FakeWhitePoint.Visibility = Visibility.Hidden;
           });

        // Menu closed
        MovementStoryboard.Events().Completed
            .Where(_ => MovementStoryboard.AutoReverse == true)
            .Subscribe(_ =>
            {
                mainWindow.Touch.IsTouchMenuOpend = isOpen = isAnimating = false;
                MovementStoryboard.AutoReverse = false;
                mainWindow.Touch.Visibility = Visibility.Visible;
                mainWindow.Touch.TouchMenuClosed.OnNext(Unit.Default);
                Visibility = Visibility.Hidden;
            });

        // Initialize animations
        var fakePointAnimation = AnimationTool.FadeOutAnimation;
        fakePointAnimation.FillBehavior = FillBehavior.HoldEnd;
        fakePointAnimation.EasingFunction = UnifiedPowerFunction;
        AnimationTool.BindingAnimation(MovementStoryboard, fakePointAnimation, FakeWhitePoint, new(OpacityProperty), true);
        AnimationTool.BindingAnimation(MovementStoryboard, AnimationTool.FadeInAnimation, MenuArea, new(OpacityProperty), true);
        AnimationTool.BindingAnimation(MovementStoryboard, TransformXAnimation, this, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(MovementStoryboard, TransformYAnimation, this, AnimationTool.YProperty);
        #endregion
    }

    /// <summary>
    /// Must initialize first and after MainWindow loaded
    /// </summary>
    private void UpdateProperties(double touchSize)
    {
        MaxHeight = MaxWidth = touchSize * 5;
        EndureEdgeHeight = MaxHeight / 10;
        XamlResource.SetAssistiveTouchItemSize(touchSize / 2);
        UpdateMenuSize(Application.Current.MainWindow.Height);

        // Update width height animation
        UpdateMenuSizeAnimation(touchSize);
    }

    /// <summary>
    /// The minimal distance between window top to menu edge
    /// </summary>
    private double EndureEdgeHeight { get; set; }

    #region X Y Width Height Animations
    private static readonly PowerEase UnifiedPowerFunction = new() { EasingMode = EasingMode.EaseInOut };
    private static readonly Storyboard MovementStoryboard = new();
    private static readonly DoubleAnimation TransformXAnimation = new()
    {
        Duration = MenuTransistDuration,
        EasingFunction = UnifiedPowerFunction
    };
    private static readonly DoubleAnimation TransformYAnimation = new()
    {
        Duration = MenuTransistDuration,
        EasingFunction = UnifiedPowerFunction
    };
    private static DoubleAnimation? _widthAnimation;
    private static DoubleAnimation? _heightAnimation;

    /// <summary>
    /// Called when menu size changed.
    /// </summary>
    /// <param name="touchSize">Dependent on touch button size</param>
    private void UpdateMenuSizeAnimation(double touchSize)
    {
        MovementStoryboard.Children.Remove(_widthAnimation);
        MovementStoryboard.Children.Remove(_heightAnimation);

        _widthAnimation = new()
        {
            From = touchSize,
            Duration = MenuTransistDuration,
            EasingFunction = UnifiedPowerFunction
        };
        _heightAnimation = new()
        {
            From = touchSize,
            Duration = MenuTransistDuration,
            EasingFunction = UnifiedPowerFunction
        };

        AnimationTool.BindingAnimation(MovementStoryboard, _widthAnimation, this, new(WidthProperty), true);
        AnimationTool.BindingAnimation(MovementStoryboard, _heightAnimation, this, new(HeightProperty), true);
    }
    #endregion

    /// <summary>
    /// When game window height changed, update this menu size.
    /// </summary>
    private void UpdateMenuSize(double newGameWindowHeight)
    {
        // The normal size of menu
        if (newGameWindowHeight > EndureEdgeHeight + MaxHeight)
        {
            Height = Width = MaxHeight;
        }
        // Small scaled size of menu
        else
        {
            var newSize = newGameWindowHeight - EndureEdgeHeight;
            Height = Width = newSize > 0 ? newSize : 0;
        }
    }

    private void PageNavigation(TouchMenuPageTag nav)
    {
        TouchMenuItem.ClickLocked = true;
        switch (nav)
        {
            case TouchMenuPageTag.Game:
                _menuMainPage.Close();
                _menuGamePage.Show(Height / 3);
                break;
            case TouchMenuPageTag.GameBack:
                _menuMainPage.Show(0);
                _menuGamePage.Close();
                break;
            case TouchMenuPageTag.Device:
                _menuMainPage.Close();
                _menuDevicePage.Show(Height / 3);
                break;
            case TouchMenuPageTag.DeviceBack:
                _menuMainPage.Show(0);
                _menuDevicePage.Close();
                break;
            case TouchMenuPageTag.Function:
                _menuMainPage.Close();
                _menuFunctionPage.Show(Height / 3);
                break;
            case TouchMenuPageTag.FunctionBack:
                _menuMainPage.Show(0);
                _menuFunctionPage.Close();
                break;
        }
    }
}
