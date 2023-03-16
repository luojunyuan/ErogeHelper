using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Entities;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.ViewModel.MainGame;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveTouch
{
    public bool IsTouchMenuOpend { get; set; }
    public Subject<Unit> TouchMenuClosed { get; set; } = new();
    public IObservable<Unit> Clicked => _clicked;

    // iPhone iPad 200ms, but 300ms more suit for eh
    private static readonly TimeSpan TouchReleaseToEdgeDuration = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan OpacityFadeInDuration = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan OpacityTransformDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan OpacityChangeDuration = TimeSpan.FromSeconds(4);

    private const double OpacityHalf = 0.4;
    private const double OpacityFull = 1;
    private const double ButtonSpace = 2;

    // The diameter of button use for mouse releasing
    private double _touchSize;
    private double _distance;
    private double _halfDistance;
    private double _oneThirdDistance;
    private double _twoThirdDistance;

    private readonly Subject<Unit> _clicked = new();

    public readonly AssistiveTouchViewModel ViewModel = null!;

    public AssistiveTouch()
    {
        InitializeComponent();

        ViewModel = new AssistiveTouchViewModel();

        ViewModel.BigTouchChanged.Subscribe(UpdateAssistiveTouchProperties);

        var mainWindow = (MainGameWindow)Application.Current.MainWindow;

        mainWindow.Events().SizeChanged
            .SkipUntil(mainWindow.Events().Loaded)
            .Subscribe(_ =>
            {
                var (x, y) = CalculateTouchMargin(_touchSize, TouchPosition, mainWindow);
                TouchPosTransform.SetCurrentValue(TranslateTransform.XProperty, x);
                TouchPosTransform.SetCurrentValue(TranslateTransform.YProperty, y);
            });

        #region Core Logic of Moving

        var lastPosRealTime = new Point(TouchPosTransform.X, TouchPosTransform.Y);
        bool isDraging = false;
        bool isMoving = false;
        bool isPendingToOpen = false;
        Point relativeMousePos;
        Point pointWhenMouseDown;
        Point pointWhenMouseUp;

        // mousedown ----> init data
        // mousemove ----> move touch ----> mouseup
        // mouseup   ----> animation

        var mouseDown = this.Events().PreviewMouseLeftButtonDown
            .Where(_ => !isMoving && !isPendingToOpen)
            .Publish();
        mouseDown.Connect();
        mouseDown.Subscribe(evt =>
        {
            isDraging = true;
            relativeMousePos = evt.GetPosition(this);
            pointWhenMouseDown = evt.GetPosition(mainWindow);
        });

        this.Events().PreviewMouseMove
            .Where(_ => isDraging)
            .Do(_ => isMoving = true)
            // Max mouse event message frequency: 125 fps, dirty react 250 (2*fps)
            .Select(evt => (Point)(evt.GetPosition(mainWindow) - relativeMousePos))
            .Subscribe(newPos =>
            {
                TouchPosTransform.SetCurrentValue(TranslateTransform.XProperty, newPos.X);
                TouchPosTransform.SetCurrentValue(TranslateTransform.YProperty, newPos.Y);

                lastPosRealTime = newPos;

                // if mouse go out of the edge
                if (newPos.X < -_oneThirdDistance || newPos.Y < -_oneThirdDistance ||
                    newPos.X > mainWindow.ActualWidth - _twoThirdDistance ||
                    newPos.Y > mainWindow.ActualHeight - _twoThirdDistance)
                {
                    RaiseMouseReleasedEventInCode(this);
                }
            });

        var mouseReleased = this.Events().PreviewMouseUp
            .Do(_ => isDraging = false)
            .Do(evt => pointWhenMouseUp = evt.GetPosition(mainWindow))
            .Select(_ => Unit.Default)
            .Publish();
        mouseReleased.Connect();
        mouseReleased
            .Where(_ => isMoving && pointWhenMouseUp != pointWhenMouseDown)
            .Subscribe(_ => WhenMouseReleased(mainWindow, TouchPosTransform.X, TouchPosTransform.Y));

        TouchMenuClosed.Subscribe(_ => isDraging = isMoving = false);

        #endregion Core Logic of Moving

        #region Opacity Adjust

        // mousedown  ----> fadein touch
        // mouseup    ----> fadeout touch
        // storyboard ----> fadeout touch

        mouseDown
            .Where(_ => Opacity != OpacityFull)
            .Subscribe(_ => BeginAnimation(OpacityProperty, FadeInOpacityAnimation));

        var animationStoped = TranslateTouchStoryboard.Events().Completed
            .Do(_ => isMoving = false)
            .Select(_ => Unit.Default);

        var mouseClicked = mouseReleased
            .Where(_ => pointWhenMouseUp == pointWhenMouseDown)
            .Do(_ => isMoving = false);

        BehaviorSubject<Unit> tryTransparentizeSubj = new(Unit.Default);
        tryTransparentizeSubj
            .Throttle(OpacityChangeDuration)
            .Where(_ => isMoving == false && IsTouchMenuOpend == false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => BeginAnimation(OpacityProperty, FadeOpacityAnimation));

        animationStoped
            .Merge(mouseClicked)
            .Merge(TouchMenuClosed)
            .Subscribe(_ => tryTransparentizeSubj.OnNext(Unit.Default));

        #endregion

        mouseClicked
            .Do(_ => isPendingToOpen = true)
            .Throttle(OpacityFadeInDuration.Multiply(2))
            .Do(_ => isPendingToOpen = false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_clicked);

        FadeOpacityAnimation.Freeze();
        FadeInOpacityAnimation.Freeze();
        AnimationTool.BindingAnimation(TranslateTouchStoryboard, TranslateXAnimation, this, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(TranslateTouchStoryboard, TranslateYAnimation, this, AnimationTool.YProperty);
        TranslateTouchStoryboard.Begin();
        TranslateTouchStoryboard.Stop();
    }

    private AssistiveTouchPosition TouchPosition
    {
        get => ViewModel.AssistiveTouchPosition;
        set => ViewModel.AssistiveTouchPosition = value;
    }

    /// <summary>
    /// Update the Touch size and reposition it in MainGameWindow, called after menu initialized
    /// </summary>
    /// <param name="useBigSize"></param>
    private void UpdateAssistiveTouchProperties(bool useBigSize)
    {
        _touchSize = useBigSize ? 100 : 60;
        XamlResource.AssistiveTouchSize = _touchSize;
        XamlResource.SetAssistiveTouchCornerRadius(new(_touchSize / 4));
        XamlResource.SetAssistiveTouchCircleLinear(new(_touchSize >= 100 ? 2 : 1.5));
        XamlResource.SetAssistiveTouchLayerOneMargin(new(_touchSize / 8));
        XamlResource.SetAssistiveTouchLayerTwoMargin(new(_touchSize * 3 / 16));
        XamlResource.SetAssistiveTouchLayerThreeMargin(new(_touchSize / 4));

        _distance = _touchSize;
        _halfDistance = _distance / 2;
        _oneThirdDistance = _distance / 3;
        _twoThirdDistance = _oneThirdDistance * 2;

        var pos = CalculateTouchMargin(_touchSize, TouchPosition, Application.Current.MainWindow);
        TouchPosTransform.SetCurrentValue(TranslateTransform.XProperty, pos.Item1);
        TouchPosTransform.SetCurrentValue(TranslateTransform.YProperty, pos.Item2);
    }

    private void WhenMouseReleased(FrameworkElement parent, double left, double top)
    {
        var parentActualHeight = parent.ActualHeight;
        var parentActualWidth = parent.ActualWidth;

        // distance of button and rifht edge
        var right = parentActualWidth - left - _touchSize;
        // distance of button and bottom edge
        var bottom = parentActualHeight - top - _touchSize;
        var verticalMiddleLine = parentActualWidth / 2;

        // determinate the position where button should be with animation
        if (left < _halfDistance && top < _twoThirdDistance) // top-left
        {
            // when the distance of button and top-left less equal than distance
            left = ButtonSpace;
            top = ButtonSpace;
            TouchPosition = AssistiveTouchPosition.UpperLeft;
        }
        else if (left < _halfDistance && bottom < _twoThirdDistance) // bottom-left
        {
            left = ButtonSpace;
            top = parentActualHeight - _touchSize - ButtonSpace;
            TouchPosition = AssistiveTouchPosition.LowerLeft;
        }
        else if (right < _halfDistance && top < _twoThirdDistance) // top-right
        {
            left = parentActualWidth - _touchSize - ButtonSpace;
            top = ButtonSpace;
            TouchPosition = AssistiveTouchPosition.UpperRight;
        }
        else if (right < _halfDistance && bottom < _twoThirdDistance) // bottom-right
        {
            left = parentActualWidth - _touchSize - ButtonSpace;
            top = parentActualHeight - _touchSize - ButtonSpace;
            TouchPosition = AssistiveTouchPosition.LowerRight;
        }
        else if (top < _twoThirdDistance) // top
        {
            top = ButtonSpace;
            TouchPosition = new()
            {
                Corner = TouchButtonCorner.Top,
                Scale = (left + ButtonSpace + (_touchSize / 2)) / parentActualWidth
            };
        }
        else if (bottom < _twoThirdDistance) // bottom
        {
            top = parentActualHeight - _touchSize - ButtonSpace;
            TouchPosition = new()
            {
                Corner = TouchButtonCorner.Bottom,
                Scale = (left + ButtonSpace + (_touchSize / 2)) / parentActualWidth
            };
        }
        else if (left + (_touchSize / 2) < verticalMiddleLine) // left
        {
            left = ButtonSpace;
            TouchPosition = new()
            {
                Corner = TouchButtonCorner.Left,
                Scale = (top + ButtonSpace + (_touchSize / 2)) / parentActualHeight
            };
        }
        else // right
        {
            left = parentActualWidth - _touchSize - ButtonSpace;
            TouchPosition = new()
            {
                Corner = TouchButtonCorner.Right,
                Scale = (top + ButtonSpace + _touchSize / 2) / parentActualHeight
            };
        }

        SmoothMoveAnimation(left, top);
    }

    private static readonly DoubleAnimation FadeOpacityAnimation = new()
    {
        From = OpacityFull,
        To = OpacityHalf,
        Duration = OpacityTransformDuration,
    };
    private static readonly DoubleAnimation FadeInOpacityAnimation = new()
    {
        From = OpacityHalf,
        To = OpacityFull,
        Duration = OpacityFadeInDuration,
    };

    private static readonly Storyboard TranslateTouchStoryboard = new();

    private static readonly DoubleAnimation TranslateXAnimation = new() { Duration = TouchReleaseToEdgeDuration };
    private static readonly DoubleAnimation TranslateYAnimation = new() { Duration = TouchReleaseToEdgeDuration };

    private static void RaiseMouseReleasedEventInCode(Button touch)
    {
        var timestamp = new TimeSpan(DateTime.Now.Ticks).Milliseconds;

        var mouseUpEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, MouseButton.Left)
        {
            RoutedEvent = PreviewMouseUpEvent,
            Source = touch,
        };

        touch.RaiseEvent(mouseUpEvent);
    }

    private static void SmoothMoveAnimation(double left, double top)
    {
        if (TranslateXAnimation.To == left)
        {
            TranslateYAnimation.To = top;
        }
        else if (TranslateYAnimation.To == top)
        {
            TranslateXAnimation.To = left;
        }
        else
        {
            TranslateXAnimation.To = left;
            TranslateYAnimation.To = top;
        }
        TranslateTouchStoryboard.Begin();
    }

    /// <summary>
    /// Get current AsisitiveTouch margin where it should be.
    /// </summary>
    private static (double, double) CalculateTouchMargin(
        double buttonSize, AssistiveTouchPosition touchPos, FrameworkElement parent)
    {
        var rightLineMargin = parent.ActualWidth - buttonSize - ButtonSpace;
        var bottomLineMargin = parent.ActualHeight - buttonSize - ButtonSpace;
        var verticalScaleMargin = (touchPos.Scale * parent.ActualHeight) - (buttonSize / 2) - ButtonSpace;
        var horizontalScaleMargin = (touchPos.Scale * parent.ActualWidth) - buttonSize / 2 - ButtonSpace;
        return touchPos.Corner switch
        {
            TouchButtonCorner.UpperLeft => (ButtonSpace, ButtonSpace),
            TouchButtonCorner.UpperRight => (rightLineMargin, ButtonSpace),
            TouchButtonCorner.LowerLeft => (ButtonSpace, bottomLineMargin),
            TouchButtonCorner.LowerRight => (rightLineMargin, bottomLineMargin),
            TouchButtonCorner.Left => (ButtonSpace, verticalScaleMargin),
            TouchButtonCorner.Top => (horizontalScaleMargin, ButtonSpace),
            TouchButtonCorner.Right => (rightLineMargin, verticalScaleMargin),
            TouchButtonCorner.Bottom => (horizontalScaleMargin, bottomLineMargin),
            _ => default,
        };
    }
}
