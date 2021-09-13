using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.Controllers
{
    /// <summary>
    /// AssistiveTouch.xaml 的交互逻辑
    /// </summary>
    public partial class AssistiveTouch : IEnableLogger
    {
        private const double OpacityValue = 0.4;
        private const double OpacityNormal = 1;
        private const double ButtonSpace = 2;

        private double Dpi => _mainWindowDataService.Dpi;
        private readonly IMainWindowDataService _mainWindowDataService;

        private FrameworkElement _parent = null!;

        /// <summary>
        /// The diameter of button
        /// </summary>
        private double _distance;
        private double _halfDistance;
        private double _oneThirdDistance;
        private double _twoThirdDistance;

        private bool _isMoving;

        private Point _lastPos;

        /// <summary>
        /// The position of the button after the left mouse button is released
        /// </summary>
        private Point _newPos;

        /// <summary>
        /// The position of the button
        /// </summary>
        private Point _oldPos;

        private AssistiveTouchPosition _touchPosition;

        public AssistiveTouch(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigRepository = null)
        {
            InitializeComponent();

            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            ehConfigRepository ??= DependencyInject.GetService<IEhConfigRepository>();

            // 不仅需要考虑窗口大小改变后，手动重置button位置，通过上次记忆的位置来重置

            _touchPosition = JsonSerializer.Deserialize<AssistiveTouchPosition>(ehConfigRepository.AssistiveTouchPosition)
                                ?? DefaultValues.TouchPosition;

            Dispatcher.Events().ShutdownStarted.Subscribe(_ =>
            {
                ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(_touchPosition);
                this.Log().Debug("Save touch position succeed");
            });

            AssistiveButton.Events().Loaded
                .Subscribe(_ =>
                {
                    if (Parent is null and not FrameworkElement)
                    {
                        throw new InvalidOperationException("Control's parent must be FrameworkElement type");
                    }

                    _parent = (Parent as FrameworkElement)!;

                    AssistiveButton.Margin = ResetButtonEdgeMargin(AssistiveButton.Width, _touchPosition, _parent);
                });

            var buttonDown = AssistiveButton.Events().PreviewMouseLeftButtonDown;
            var disableFlyoutPopup = this.Events().PreviewMouseUp;

            var mouseMove = this.Events().PreviewMouseMove;
            var mouseUp = this.Events().PreviewMouseUp;

            buttonDown.Subscribe(evt =>
            {
                AssistiveButton.Opacity = OpacityNormal;
                _isMoving = true;
                _lastPos = evt.GetPosition(_parent);
                _oldPos = _lastPos;
            });

            mouseMove.Subscribe(evt =>
            {
                if (!_isMoving)
                    return;

                var pos = evt.GetPosition(_parent);
                // 相对左上坐标 + 新位置与旧位置的差值 = 新坐标
                var left = AssistiveButton.Margin.Left + pos.X - _lastPos.X;
                var top = AssistiveButton.Margin.Top + pos.Y - _lastPos.Y;

                // dummy problem UserControl would clip if Button get out of the bottom of window
                var workaround = 0.0;
                var outerBounds = top + AssistiveButton.Height;
                if (outerBounds > _parent.ActualHeight)
                {
                    workaround = _parent.ActualHeight - outerBounds;
                }

                AssistiveButton.Margin = new Thickness(left, top, 0, workaround);

                _lastPos = pos;

                // When mouse out the edge
                if (left < -_oneThirdDistance || top < -_oneThirdDistance ||
                    left > _parent.ActualWidth - _twoThirdDistance || top > _parent.ActualHeight - _twoThirdDistance)
                {
                    RaiseMouseUpEventInCode(AssistiveButton, this);
                }
            });

            mouseUp.Subscribe(evt =>
            {
                if (!_isMoving)
                    return;

                double parentActualWidth;
                double parentActualHeight;
                double curMarginLeft;
                double curMarginTop;

                var pos = evt.GetPosition(_parent);
                _newPos = pos;
                parentActualHeight = _parent.ActualHeight;
                parentActualWidth = _parent.ActualWidth;
                curMarginLeft = AssistiveButton.Margin.Left;
                curMarginTop = AssistiveButton.Margin.Top;

                var left = curMarginLeft + _newPos.X - _lastPos.X;
                var top = curMarginTop + _newPos.Y - _lastPos.Y;
                // button 距离右边缘距离
                var right = parentActualWidth - left - AssistiveButton.Width;
                // button 距离下边缘距离
                var bottom = parentActualHeight - top - AssistiveButton.Height;
                var verticalMiddleLine = parentActualWidth / 2;

                // 根据button所处屏幕位置来确定button之后应该动画移动到的位置
                if (left < _halfDistance && top < _twoThirdDistance) // button 距离左上角边距同时小于 distance
                {
                    left = ButtonSpace;
                    top = ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.UpperLeft);
                }
                else if (left < _halfDistance && bottom < _twoThirdDistance) // bottom-left
                {
                    left = ButtonSpace;
                    top = parentActualHeight - AssistiveButton.Height - ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerLeft);
                }
                else if (right < _halfDistance && top < _twoThirdDistance) // top-right
                {
                    left = parentActualWidth - AssistiveButton.Width - ButtonSpace;
                    top = ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.UpperRight);
                }
                else if (right < _halfDistance && bottom < _twoThirdDistance) // bottom-right
                {
                    left = parentActualWidth - AssistiveButton.Width - ButtonSpace;
                    top = parentActualHeight - AssistiveButton.Height - ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerRight);
                }
                else if (top < _twoThirdDistance) // top
                {
                    left = curMarginLeft;
                    top = ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + AssistiveButton.Width / 2) / parentActualWidth;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Top, scale);
                }
                else if (bottom < _twoThirdDistance) // bottom
                {
                    left = curMarginLeft;
                    top = parentActualHeight - AssistiveButton.Height - ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + (AssistiveButton.Width / 2)) / parentActualWidth;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Bottom, scale);
                }
                else if (left + (AssistiveButton.Width / 2) < verticalMiddleLine) // left
                {
                    left = ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + (AssistiveButton.Height / 2)) / parentActualHeight;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Left, scale);
                }
                else // right
                {
                    left = parentActualWidth - AssistiveButton.Width - ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + AssistiveButton.Height / 2) / parentActualHeight;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Right, scale);
                }

                SmoothMoveAnimation(AssistiveButton, left, top);
                _isMoving = false;
            });

            disableFlyoutPopup.Subscribe(evt =>
            {
                if (!_newPos.Equals(_oldPos) && evt is not null)
                {
                    evt.Handled = true;
                }
            });

            // Refactor
            this.WhenAnyValue(x => x.AssistiveButton.Width)
                .Where(w => !double.IsNaN(w))
                .Subscribe(width =>
                {
                    ButtonSizePropUpdate(width);
                    if (_parent is not null)
                    {
                        AssistiveButton.Margin = ResetButtonEdgeMargin(width, DefaultValues.TouchPosition, _parent);
                    }
                });

            this.WhenActivated(d =>
            {
                //this.OneWayBind(ViewModel,
                //    vm => vm.AssistiveTouchSize,
                //    v => v.AssistiveButton.Width)
                //    .DisposeWith(d);
                //this.OneWayBind(ViewModel,
                //    vm => vm.AssistiveTouchSize,
                //    v => v.AssistiveButton.Height)
                //    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.TestCommand,
                    v => v.TestButton)
                    .DisposeWith(d);
            });
        }

        private void ButtonSizePropUpdate(double width)
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

        private static Thickness ResetButtonEdgeMargin(double buttonSize, AssistiveTouchPosition pos, FrameworkElement parent)
        {
            var rightLineMargin = parent.ActualWidth - buttonSize - ButtonSpace;
            var bottomLineMargin = parent.ActualHeight - buttonSize - ButtonSpace;
            var verticalScaleMargin = (pos.Scale * parent.ActualHeight) - (buttonSize / 2) - ButtonSpace;
            var horizontalScaleMargin = pos.Scale * parent.ActualWidth - buttonSize / 2 - ButtonSpace;
            return pos.Corner switch
            {
                TouchButtonCorner.UpperLeft => new Thickness(ButtonSpace, ButtonSpace, 0, 0),
                TouchButtonCorner.UpperRight => new Thickness(rightLineMargin, ButtonSpace, 0, 0),
                TouchButtonCorner.LowerLeft => new Thickness(ButtonSpace, bottomLineMargin, 0, 0),
                TouchButtonCorner.LowerRight => new Thickness(rightLineMargin, bottomLineMargin, 0, 0),
                TouchButtonCorner.Left => new Thickness(ButtonSpace, verticalScaleMargin, 0, 0),
                TouchButtonCorner.Top => new Thickness(horizontalScaleMargin, ButtonSpace, 0, 0),
                TouchButtonCorner.Right => new Thickness(rightLineMargin, verticalScaleMargin, 0, 0),
                TouchButtonCorner.Bottom => new Thickness(bottomLineMargin, horizontalScaleMargin, 0, 0),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}