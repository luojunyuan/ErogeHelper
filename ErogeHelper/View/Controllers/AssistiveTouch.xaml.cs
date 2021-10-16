using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Enums;
using ErogeHelper.ViewModel.Controllers;
using ModernWpf.Controls.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.Controllers
{
    public partial class AssistiveTouch : IEnableLogger
    {
        private const double OpacityValue = 0.4;
        private const double OpacityNormal = 1;
        private const double ButtonSpace = 2;

        // The diameter of button use for orienting input interaction
        private double _distance;
        private double _halfDistance;
        private double _oneThirdDistance;
        private double _twoThirdDistance;

        // XXX: readonly can also be bind reactive
#pragma warning disable CS0649
        private readonly double _buttonSize;

        public AssistiveTouch(AssistiveTouchViewModel? assistiveTouchViewModel = null)
        {
            InitializeComponent();

            ViewModel = assistiveTouchViewModel ?? DependencyResolver.GetService<AssistiveTouchViewModel>();

            var touchPosition = ViewModel!.AssistiveTouchPosition;
            Dispatcher.Events().ShutdownStarted.Subscribe(_ =>
            {
                ViewModel!.AssistiveTouchPosition = touchPosition;
                this.Log().Debug("Save touch position succeed");
            });


            FrameworkElement? parent = null;

            AssistiveButton.Events().Loaded
                .Subscribe(_ =>
                {
                    parent ??= Parent is not FrameworkElement reactiveViewModelViewHost
                        ? throw new InvalidOperationException("Control's parent must be FrameworkElement type")
                        : reactiveViewModelViewHost;

                    ViewModel!.ParentFrameWorkElementLoaded = true;
                    UpdateButtonDiameterProperties(_buttonSize);
                    AssistiveButton.Margin = GetButtonEdgeMargin(_buttonSize, touchPosition, parent);
                    AssistiveTouchFlyout.Placement = GetFlyoutPlacement(touchPosition);
                });

            #region Opacity Adjust
            Point lastPos;
            var isMoving = false;
            BehaviorSubject<bool> tryTransparentizeSubj = new(true);
            tryTransparentizeSubj
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.AssistiveTouchOpacityChangedTimeout))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(on => on && isMoving == false && AssistiveTouchFlyout.IsOpen == false)
                .Subscribe(_ => AssistiveButton.Opacity = OpacityValue);

            AssistiveTouchFlyout.Events().Closed
                .Subscribe(_ => tryTransparentizeSubj.OnNext(true));
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
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.UpperLeft);
                }
                else if (left < _halfDistance && bottom < _twoThirdDistance) // bottom-left
                {
                    left = ButtonSpace;
                    top = parentActualHeight - _buttonSize - ButtonSpace;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerLeft);
                }
                else if (right < _halfDistance && top < _twoThirdDistance) // top-right
                {
                    left = parentActualWidth - _buttonSize - ButtonSpace;
                    top = ButtonSpace;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.UpperRight);
                }
                else if (right < _halfDistance && bottom < _twoThirdDistance) // bottom-right
                {
                    left = parentActualWidth - _buttonSize - ButtonSpace;
                    top = parentActualHeight - _buttonSize - ButtonSpace;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerRight);
                }
                else if (top < _twoThirdDistance) // top
                {
                    left = curMarginLeft;
                    top = ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + (_buttonSize / 2)) / parentActualWidth;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Top, scale);
                }
                else if (bottom < _twoThirdDistance) // bottom
                {
                    left = curMarginLeft;
                    top = parentActualHeight - _buttonSize - ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + (_buttonSize / 2)) / parentActualWidth;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Bottom, scale);
                }
                else if (left + (_buttonSize / 2) < verticalMiddleLine) // left
                {
                    left = ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + (_buttonSize / 2)) / parentActualHeight;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Left, scale);
                }
                else // right
                {
                    left = parentActualWidth - _buttonSize - ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + _buttonSize / 2) / parentActualHeight;
                    touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Right, scale);
                }

                SmoothMoveAnimation(AssistiveButton, left, top);
                isMoving = false;
                tryTransparentizeSubj.OnNext(true);
                AssistiveTouchFlyout.Placement = GetFlyoutPlacement(touchPosition);
            });
            #endregion

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.AssistiveTouchTemplate,
                    v => v.AssistiveButton.Template).DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.AssistiveTouchVisibility,
                    v => v.AssistiveButton.Visibility).DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ButtonSize,
                    v => v._buttonSize).DisposeWith(d);

                this.WhenAnyObservable(x => x.ViewModel!.UpdateButtonDiameterSubj)
                    .Subscribe(buttonSize => UpdateButtonDiameterProperties(buttonSize)).DisposeWith(d);

                this.WhenAnyObservable(x => x.ViewModel!.SetButtonDockPosSubj)
                    .Subscribe(_ => AssistiveButton.Margin = GetButtonEdgeMargin(_buttonSize, touchPosition, parent!)).DisposeWith(d);

                this.WhenAnyObservable(x => x.ViewModel!.HideFlyoutSubj)
                    .Subscribe(_ => AssistiveTouchFlyout.Hide()).DisposeWith(d);

                // Flyout Commands
                this.Bind(ViewModel,
                    vm => vm.LoseFocusIsOn,
                    v => v.LoseFocusToggle.IsChecked).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.TouchBoxIsOn,
                    v => v.TouchBoxToggle.IsChecked).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.TouchBoxVisible,
                    v => v.TouchBoxToggle.Visibility,
                    on => on ? Visibility.Visible : Visibility.Collapsed,
                    visible => visible == Visibility.Visible).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.IsTouchToMouse,
                    v => v.TouchConversionToggle.IsChecked).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.VolumeDown,
                    v => v.VolumeDown).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.VolumeUp,
                    v => v.VolumeUp).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.SwitchFullScreen,
                    v => v.FullScreenSwitcher).DisposeWith(d);
                this.OneWayBind(ViewModel,
                  vm => vm.SwitchFullScreenIcon,
                  v => v.FullScreenSwitcher.Icon).DisposeWith(d);
                this.OneWayBind(ViewModel,
                  vm => vm.SwitchFullScreenToolTip,
                  v => v.FullScreenSwitcher.ToolTip).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.TaskbarNotifyArea,
                    v => v.ActionCenter).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.TaskView,
                    v => v.TaskView).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.ScreenShot,
                    v => v.ScreenShot).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.OpenPreference,
                    v => v.Preference).DisposeWith(d);
                // Custom apearrence: ButtonSpace, square to circle, size of the square 
            });
        }

        private void UpdateButtonDiameterProperties(double width)
        {
            _distance = width;
            _halfDistance = _distance / 2;
            _oneThirdDistance = _distance / 3;
            _twoThirdDistance = _oneThirdDistance * 2;
        }

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
            double buttonSize, AssistiveTouchPosition pos, FrameworkElement parent)
        {
            var rightLineMargin = parent.ActualWidth - buttonSize - ButtonSpace;
            var bottomLineMargin = parent.ActualHeight - buttonSize - ButtonSpace;
            var verticalScaleMargin = (pos.Scale * parent.ActualHeight) - (buttonSize / 2) - ButtonSpace;
            var horizontalScaleMargin = (pos.Scale * parent.ActualWidth) - buttonSize / 2 - ButtonSpace;
            return pos.Corner switch
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

        private static FlyoutPlacementMode GetFlyoutPlacement(AssistiveTouchPosition touchPosition) =>
            touchPosition.Corner is
            TouchButtonCorner.Right or TouchButtonCorner.UpperRight or TouchButtonCorner.LowerRight
            ? FlyoutPlacementMode.LeftEdgeAlignedTop
            : ((touchPosition.Corner == TouchButtonCorner.Top && touchPosition.Scale > 0.5) ||
               (touchPosition.Corner == TouchButtonCorner.Bottom && touchPosition.Scale > 0.5))
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
    }
}