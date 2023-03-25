using ErogeHelper.AssistiveTouch.Helper;
using ErogeHelper.AssistiveTouch.Menu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ErogeHelper.AssistiveTouch
{
    public partial class TouchMenu : Border
    {
        public static TimeSpan MenuTransistDuration { get; } = TimeSpan.FromMilliseconds(200);

        private readonly ITouchMenuPage _menuMainPage = new MainPage();
        private readonly ITouchMenuPage _menuDevicePage = new DevicePage();
        private readonly ITouchMenuPage _menuGamePage = new GamePage();
        private readonly ITouchMenuPage _menuWinMovePage = new WinMovePage();
        // also add visibility status in Move Logic

        public TouchMenu()
        {
            InitializeComponent();
            XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

            MainMenu.Navigate(_menuMainPage);
            GameMenu.Navigate(_menuGamePage);
            DeviceMenu.Navigate(_menuDevicePage);
            WinMoveMenu.Navigate(_menuWinMovePage);

            _menuMainPage.PageChanged += (s, e) => PageNavigation(e.Tag);
            _menuGamePage.PageChanged += (s, e) => PageNavigation(e.Tag);
            _menuDevicePage.PageChanged += (s, e) => PageNavigation(e.Tag);
            _menuWinMovePage.PageChanged += (s, e) => PageNavigation(e.Tag);

            var mainWindow = (MainWindow)Application.Current.MainWindow;

            Loaded += (_, _) => UpdateProperties(TouchButton.TouchSize);

            Loaded += (_, _) => mainWindow.SizeChanged += (_, e) =>
            {
                if (e.HeightChanged && e.NewSize.Height > EndureEdgeHeight)
                {
                    UpdateMenuSize(e.NewSize.Height);
                }
            };

            #region Move Logic
            var isOpen = false;
            var isAnimating = false;

            mainWindow.Touch.Clicked += (_, _) =>
            {
                TouchMenuItem.ClickLocked = true;
                Visibility = Visibility.Visible;
                mainWindow.Touch.Visibility = Visibility.Hidden;
                mainWindow.Touch.IsTouchMenuOpend = isOpen = isAnimating = true;

                _menuMainPage.Visibility = Visibility.Visible;
                _menuGamePage.Visibility = Visibility.Collapsed;
                _menuDevicePage.Visibility = Visibility.Collapsed;
                _menuWinMovePage.Visibility = Visibility.Collapsed;

                var relativeBeginX = mainWindow.Width / 2 - mainWindow.Touch.ActualWidth / 2;
                var relativeBeginY = mainWindow.Height / 2 - mainWindow.Touch.ActualWidth / 2;

                TransformXAnimation.From = mainWindow.Touch.TouchPosTransform.X - relativeBeginX;
                TransformYAnimation.From = mainWindow.Touch.TouchPosTransform.Y - relativeBeginY;
                MovementStoryboard.Begin();
            };

            void CloseMenuInternel()
            {
                if (!isAnimating)
                {
                    isAnimating = true;
                    FakeWhitePoint.Visibility = Visibility.Visible;
                    MovementStoryboard.AutoReverse = true;
                    MovementStoryboard.Begin();
                    MovementStoryboard.Seek(MenuTransistDuration);
                }
            }
            mainWindow.Deactivated += (_, _) => { if (isOpen == true) CloseMenuInternel(); };
            PreviewMouseLeftButtonUp += (_, e) => { if (e.OriginalSource is TouchMenu && !TouchMenuItem.ClickLocked) CloseMenuInternel(); };

            // Mene opend
            MovementStoryboard.Completed += (_, _) =>
            {
                if (MovementStoryboard.AutoReverse == false)
                {
                    isAnimating = false;
                    TouchMenuItem.ClickLocked = false;
                    FakeWhitePoint.Visibility = Visibility.Hidden;
                }
            };

            // Menu closed
            MovementStoryboard.Completed += (_, _) =>
            {
                // FIXEME: after window size change the touch position 
                if (MovementStoryboard.AutoReverse == true)
                {
                    mainWindow.Touch.IsTouchMenuOpend = isOpen = isAnimating = false;
                    MovementStoryboard.AutoReverse = false;
                    mainWindow.Touch.Visibility = Visibility.Visible;
                    mainWindow.Touch.RaiseMenuClosedEvent(this);
                    Visibility = Visibility.Hidden;
                }
            };
            #endregion

            // Initialize animations
            var fakePointAnimation = AnimationTool.FadeOutAnimation;
            fakePointAnimation.FillBehavior = FillBehavior.HoldEnd;
            fakePointAnimation.EasingFunction = UnifiedPowerFunction;
            AnimationTool.BindingAnimation(MovementStoryboard, fakePointAnimation, FakeWhitePoint, new(OpacityProperty), true);
            AnimationTool.BindingAnimation(MovementStoryboard, AnimationTool.FadeInAnimation, MenuArea, new(OpacityProperty), true);
            AnimationTool.BindingAnimation(MovementStoryboard, TransformXAnimation, this, AnimationTool.XProperty);
            AnimationTool.BindingAnimation(MovementStoryboard, TransformYAnimation, this, AnimationTool.YProperty);
        }

        public void ManualClose()
        {
            FakeWhitePoint.Visibility = Visibility.Visible;
            MovementStoryboard.AutoReverse = true;
            MovementStoryboard.Begin();
            MovementStoryboard.Seek(MenuTransistDuration);
        }

        /// <summary>
        /// Must initialize first after MainWindow loaded
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

                case TouchMenuPageTag.WinMove:
                    _menuWinMovePage.Show(Height / 3);
                    break;
            }
        }
    }
}
