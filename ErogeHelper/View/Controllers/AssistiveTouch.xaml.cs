using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ErogeHelper.Functions;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using ErogeHelper.ViewModel.Windows;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using WindowsInput.Events;

namespace ErogeHelper.View.Controllers;

public partial class AssistiveTouch : IEnableLogger
{
    private const double OpacityValue = 0.4;
    private const double OpacityNormal = 1;
    private const double ButtonSpace = 2;

    // The diameter of button use for mouse releasing
    private double _distance;
    private double _halfDistance;
    private double _oneThirdDistance;
    private double _twoThirdDistance;

    private double _buttonSize;

    public AssistiveTouch()
    {
        InitializeComponent();

        FrameworkElement? parent = null;
        CompositeDisposable disposables = new();
        AssistiveTouchPosition touchPosition = null!;

        AssistiveButton.Loaded += (_, _) =>
        {
            parent ??= Parent is not FrameworkElement reactiveViewModelViewHost
                    ? throw new InvalidOperationException("Control's parent must be FrameworkElement type")
                    : reactiveViewModelViewHost;

            touchPosition = ViewModel!.AssistiveTouchPosition;

            UpdateButtonDiameterProperties(ViewModel.IsTouchBigSize, touchPosition, parent);

            parent.Events().SizeChanged
                .Subscribe(_ => AssistiveButton.Margin = GetButtonEdgeMargin(_buttonSize, touchPosition, parent))
                .DisposeWith(disposables);
        };

        #region Opacity Adjust
        Point lastPos;
        var isMoving = false;
        BehaviorSubject<bool> tryTransparentizeSubj = new(true);
        tryTransparentizeSubj
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.AssistiveTouchOpacityChangedTime))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(on => on && isMoving == false && AssistiveTouchFlyout.IsOpen == false)
            .Subscribe(_ => AssistiveButton.Opacity = OpacityValue);

        AssistiveTouchFlyout.Events().Closed
            .Subscribe(_ => tryTransparentizeSubj.OnNext(true))
            .DisposeWith(disposables);
        #endregion

        #region Core Logic of Moving

        var updateStatusWhenMouseDown = AssistiveButton.Events().PreviewMouseLeftButtonDown;
        var mouseMoveAndCheckEdge = this.Events().PreviewMouseMove;
        var mouseRelease = this.Events().PreviewMouseUp;

        updateStatusWhenMouseDown.Subscribe(evt =>
        {
            AssistiveButton.Opacity = OpacityNormal;
            isMoving = true;
            tryTransparentizeSubj.OnNext(true);
            lastPos = evt.GetPosition(parent);
            _oldPos = lastPos;
        });

        mouseMoveAndCheckEdge.Subscribe(evt =>
        {
            if (!isMoving || parent is null)
                return;

            var pos = evt.GetPosition(parent);
            // 相对左上坐标 + 新位置与旧位置的差值 = 新坐标
            var left = AssistiveButton.Margin.Left + pos.X - lastPos.X;
            var top = AssistiveButton.Margin.Top + pos.Y - lastPos.Y;

            // dummy problem UserControl would clip if Button get out of the bottom of window
            var workaround = 0.0;
            var outerBounds = top + _buttonSize;
            if (outerBounds > parent.ActualHeight)
            {
                workaround = parent.ActualHeight - outerBounds;
            }

            AssistiveButton.Margin = new Thickness(left, top, 0, workaround);

            lastPos = pos;

            // When mouse out the edge
            if (left < -_oneThirdDistance || top < -_oneThirdDistance ||
            left > parent.ActualWidth - _twoThirdDistance || top > parent.ActualHeight - _twoThirdDistance)
            {
                RaiseMouseUpEventInCode(AssistiveButton, this);
            }
        });

        mouseRelease.Subscribe(evt =>
        {
            if (!isMoving || parent is null)
                return;

            double parentActualWidth;
            double parentActualHeight;
            double curMarginLeft;
            double curMarginTop;

            var pos = evt.GetPosition(parent);
            _newPos = pos;
            parentActualHeight = parent.ActualHeight;
            parentActualWidth = parent.ActualWidth;
            curMarginLeft = AssistiveButton.Margin.Left;
            curMarginTop = AssistiveButton.Margin.Top;

            var left = curMarginLeft + _newPos.X - lastPos.X;
            var top = curMarginTop + _newPos.Y - lastPos.Y;
            // button 距离右边缘距离
            var right = parentActualWidth - left - _buttonSize;
            // button 距离下边缘距离
            var bottom = parentActualHeight - top - _buttonSize;
            var verticalMiddleLine = parentActualWidth / 2;

            // 根据button所处屏幕位置来确定button之后应该动画移动到的位置
            if (left < _halfDistance && top < _twoThirdDistance) // button 距离左上角边距同时小于 distance
            {
                left = ButtonSpace;
                top = ButtonSpace;
                touchPosition = AssistiveTouchPosition.UpperLeft;
            }
            else if (left < _halfDistance && bottom < _twoThirdDistance) // bottom-left
            {
                left = ButtonSpace;
                top = parentActualHeight - _buttonSize - ButtonSpace;
                touchPosition = AssistiveTouchPosition.LowerLeft;
            }
            else if (right < _halfDistance && top < _twoThirdDistance) // top-right
            {
                left = parentActualWidth - _buttonSize - ButtonSpace;
                top = ButtonSpace;
                touchPosition = AssistiveTouchPosition.UpperRight;
            }
            else if (right < _halfDistance && bottom < _twoThirdDistance) // bottom-right
            {
                left = parentActualWidth - _buttonSize - ButtonSpace;
                top = parentActualHeight - _buttonSize - ButtonSpace;
                touchPosition = AssistiveTouchPosition.LowerRight;
            }
            else if (top < _twoThirdDistance) // top
            {
                left = curMarginLeft;
                top = ButtonSpace;
                var scale = (curMarginLeft + ButtonSpace + (_buttonSize / 2)) / parentActualWidth;
                touchPosition.Corner = TouchButtonCorner.Top;
                touchPosition.Scale = scale;
            }
            else if (bottom < _twoThirdDistance) // bottom
            {
                left = curMarginLeft;
                top = parentActualHeight - _buttonSize - ButtonSpace;
                var scale = (curMarginLeft + ButtonSpace + (_buttonSize / 2)) / parentActualWidth;
                touchPosition.Corner = TouchButtonCorner.Bottom;
                touchPosition.Scale = scale;
            }
            else if (left + (_buttonSize / 2) < verticalMiddleLine) // left
            {
                left = ButtonSpace;
                top = curMarginTop;
                var scale = (curMarginTop + ButtonSpace + (_buttonSize / 2)) / parentActualHeight;
                touchPosition.Corner = TouchButtonCorner.Left;
                touchPosition.Scale = scale;
            }
            else // right
            {
                left = parentActualWidth - _buttonSize - ButtonSpace;
                top = curMarginTop;
                var scale = (curMarginTop + ButtonSpace + _buttonSize / 2) / parentActualHeight;
                touchPosition.Corner = TouchButtonCorner.Right;
                touchPosition.Scale = scale;
            }

            SmoothMoveAnimation(AssistiveButton, left, top);
            isMoving = false;
            tryTransparentizeSubj.OnNext(true);
            AssistiveTouchFlyout.Placement = GetFlyoutPlacement(touchPosition);
            ViewModel!.AssistiveTouchPositionChanged.OnNext(touchPosition);
        });

        #endregion Core Logic of Moving

        this.WhenActivated(d =>
        {
            disposables.DisposeWith(d);
            ViewModel!.DisposeWith(d);

            this.WhenAnyObservable(x => x.ViewModel!.UseBigSize)
                .Subscribe(isBigSize =>
                    UpdateButtonDiameterProperties(isBigSize, touchPosition, parent!)).DisposeWith(d);

            var backToWindowIcon = new SymbolIcon { Symbol = Symbol.BackToWindow };
            var fullScreenIcon = new SymbolIcon { Symbol = Symbol.FullScreen };
            this.OneWayBind(ViewModel,
              vm => vm.SwitchFullScreenIcon,
              v => v.FullScreenSwitcher.Icon,
              iconEnum => iconEnum switch
              {
                  SymbolName.BackToWindow => backToWindowIcon,
                  SymbolName.FullScreen => fullScreenIcon,
                  _ => throw new InvalidCastException(),
              }).DisposeWith(d);
            this.OneWayBind(ViewModel,
              vm => vm.SwitchFullScreenToolTip,
              v => v.FullScreenSwitcher.ToolTip).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.LoseFocusEnable,
                v => v.LoseFocusToggle.IsChecked).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.TouchBoxEnable,
                v => v.TouchBoxToggle.IsChecked).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.TouchBoxSwitcherVisible,
                v => v.TouchBoxToggle.Visibility,
                on => on ? Visibility.Visible : Visibility.Collapsed,
                visible => visible == Visibility.Visible).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.IsTouchToMouse,
                v => v.TouchConversionToggle.IsChecked).DisposeWith(d);

            // Custom apearrence: ButtonSpace, square to circle, size of the square 
        });
    }

    private static readonly double AssistiveTouchSize =
        (double)Application.Current.Resources["AssistiveTouchSize"];
    private static readonly double AssistiveTouchBigSize =
        (double)Application.Current.Resources["BigAssistiveTouchSize"];

    private void UpdateButtonDiameterProperties
        (bool isBigSize, AssistiveTouchPosition touchPos, FrameworkElement parent)
    {
        AssistiveButton.Template = GetAssistiveTouchStyle(isBigSize);
        _buttonSize = isBigSize ? AssistiveTouchBigSize : AssistiveTouchSize;
        _distance = _buttonSize;
        _halfDistance = _distance / 2;
        _oneThirdDistance = _distance / 3;
        _twoThirdDistance = _oneThirdDistance * 2;

        AssistiveButton.Margin = GetButtonEdgeMargin(_buttonSize, touchPos, parent);
        AssistiveTouchFlyout.Placement = GetFlyoutPlacement(touchPos);
    }

    private static ControlTemplate GetAssistiveTouchStyle(bool useBigSize) =>
       useBigSize ? Application.Current.Resources["BigAssistiveTouchTemplate"]
                       as ControlTemplate
                       ?? throw new IOException("Cannot locate resource 'BigAssistiveTouchTemplate'")
                  : Application.Current.Resources["NormalAssistiveTouchTemplate"]
                       as ControlTemplate
                       ?? throw new IOException("Cannot locate resource 'NormalAssistiveTouchTemplate'");

    private static void RaiseMouseUpEventInCode(Button button, UIElement father)
    {
        var timestamp = new TimeSpan(DateTime.Now.Ticks).Milliseconds;

        var mouseUpEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, MouseButton.Left)
        {
            RoutedEvent = PreviewMouseUpEvent,
            Source = button,
        };

        father.RaiseEvent(mouseUpEvent);
    }

    private static void SmoothMoveAnimation(Button button, double left, double top)
    {
        var marginAnimation = new ThicknessAnimation
        {
            From = button.Margin,
            To = new Thickness(left, top, 0, 0),
            Duration = TimeSpan.FromMilliseconds(300)
        };

        Storyboard story = new()
        {
            FillBehavior = FillBehavior.Stop
        };
        story.Children.Add(marginAnimation);
        Storyboard.SetTargetName(marginAnimation, nameof(AssistiveButton));
        Storyboard.SetTargetProperty(marginAnimation, new PropertyPath("(0)", MarginProperty));

        story.Begin(button);

        button.Margin = new Thickness(left, top, 0, 0);
    }

    private static Thickness GetButtonEdgeMargin(
        double buttonSize, AssistiveTouchPosition touchPos, FrameworkElement parent)
    {
        var rightLineMargin = parent.ActualWidth - buttonSize - ButtonSpace;
        var bottomLineMargin = parent.ActualHeight - buttonSize - ButtonSpace;
        var verticalScaleMargin = (touchPos.Scale * parent.ActualHeight) - (buttonSize / 2) - ButtonSpace;
        var horizontalScaleMargin = (touchPos.Scale * parent.ActualWidth) - buttonSize / 2 - ButtonSpace;
        return touchPos.Corner switch
        {
            TouchButtonCorner.UpperLeft => new Thickness(ButtonSpace, ButtonSpace, 0, 0),
            TouchButtonCorner.UpperRight => new Thickness(rightLineMargin, ButtonSpace, 0, 0),
            TouchButtonCorner.LowerLeft => new Thickness(ButtonSpace, bottomLineMargin, 0, 0),
            TouchButtonCorner.LowerRight => new Thickness(rightLineMargin, bottomLineMargin, 0, 0),
            TouchButtonCorner.Left => new Thickness(ButtonSpace, verticalScaleMargin, 0, 0),
            TouchButtonCorner.Top => new Thickness(horizontalScaleMargin, ButtonSpace, 0, 0),
            TouchButtonCorner.Right => new Thickness(rightLineMargin, verticalScaleMargin, 0, 0),
            TouchButtonCorner.Bottom => new Thickness(horizontalScaleMargin, bottomLineMargin, 0, 0),
            _ => throw new InvalidOperationException(),
        };
    }

    private static FlyoutPlacementMode GetFlyoutPlacement(AssistiveTouchPosition touchPos) =>
        touchPos.Corner is
        TouchButtonCorner.Right or TouchButtonCorner.UpperRight or TouchButtonCorner.LowerRight
        ? FlyoutPlacementMode.LeftEdgeAlignedTop
        : ((touchPos.Corner == TouchButtonCorner.Top && touchPos.Scale > 0.5) ||
           (touchPos.Corner == TouchButtonCorner.Bottom && touchPos.Scale > 0.5))
        ? FlyoutPlacementMode.LeftEdgeAlignedTop
        : FlyoutPlacementMode.RightEdgeAlignedTop;

    private Point _newPos; // The position of the button after the left mouse button is released
    private Point _oldPos; // The position of the button

    // NOTE: The OnButtonClick() in FlyoutService is happend between this delegate and lambda expression
    private void DisableFlyoutPopup(object _, RoutedEventArgs e)
    {
        if (!_newPos.Equals(_oldPos) && e is not null)
        {
            e.Handled = true;
        }
    }

    private async void VolumeDownOnClick(object sender, RoutedEventArgs e) =>
         await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeDown)
            .Invoke().ConfigureAwait(false);

    private async void VolumeUpOnClick(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeUp)
            .Invoke().ConfigureAwait(false);

    private async void FullScreenSwitcherOnClick(object sender, RoutedEventArgs e)
    {
        // アインシュタイン not work
        ViewModel!.SwitchFullScreen.Execute(Unit.Default).Subscribe();
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.Alt, KeyCode.Enter)
            .Invoke().ConfigureAwait(false);
    }

    private async void ActionCenterOnClick(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.A)
            .Invoke().ConfigureAwait(false);

    private async void TaskViewOnClick(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab)
            .Invoke().ConfigureAwait(false);

    private async void ScreenShotOnClick(object sender, RoutedEventArgs e)
    {
        AssistiveTouchFlyout.Hide();
        Visibility = Visibility.Collapsed;

        await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S)
            .Invoke().ConfigureAwait(false);

        await Task.Delay(ConstantValue.ScreenShotHideButtonTime).ConfigureAwait(true);

        Visibility = Visibility.Visible;
    }

    private void HookSettingOnClick(object sender, RoutedEventArgs e)
    {
        AssistiveTouchFlyout.Hide();
        DI.ShowView<HookViewModel>();
    }

    private void PreferenceOnClick(object sender, RoutedEventArgs e) =>
        DI.ShowView<PreferenceViewModel>();
}
