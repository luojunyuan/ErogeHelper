using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.ViewModel.Controllers;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Vanara.PInvoke;

namespace ErogeHelper.View.Controllers
{
    public partial class AssistiveTouch : IEnableLogger
    {
        private const double OpacityValue = 0.4;
        private const double OpacityNormal = 1;
        private const double ButtonSpace = 2;

        private readonly IMainWindowDataService _mainWindowDataService;
        private readonly IEhConfigRepository _ehConfigRepository;

        private HWND MainWindowHandle => _mainWindowDataService.Handle;

        private double ButtonSize => _ehConfigRepository.UseBigAssistiveTouchSize ?
            DefaultValues.AssistiveTouchBigSize :
            DefaultValues.AssistiveTouchSize;

        public AssistiveTouch(
            IMainWindowDataService? mainWindowDataService = null,
            IEhConfigRepository? ehConfigRepository = null,
            IGameWindowHooker? gameWindowHooker = null,
            IGameDataService? gameDataService = null)
        {
            InitializeComponent();

            _mainWindowDataService = mainWindowDataService ?? DependencyInject.GetService<IMainWindowDataService>();
            _ehConfigRepository = ehConfigRepository ?? DependencyInject.GetService<IEhConfigRepository>();
            gameWindowHooker ??= DependencyInject.GetService<IGameWindowHooker>();
            gameDataService ??= DependencyInject.GetService<IGameDataService>();

            // ANNOYING: Constructor is too long
            var _touchPosition = JsonSerializer.Deserialize<AssistiveTouchPosition>(_ehConfigRepository.AssistiveTouchPosition)
                                ?? DefaultValues.TouchPosition;

            Dispatcher.Events().ShutdownStarted.Subscribe(_ =>
            {
                _ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(_touchPosition);
                this.Log().Debug("Save touch position succeed");
            });

            #region Updates of Fullscreen, WindowSize, DPI Changed 
            BehaviorSubject<bool> _stayTopSubject = new(Utils.IsGameForegroundFullscreen(gameDataService.MainWindowHandle));

            var interval = Observable
                .Interval(TimeSpan.FromMilliseconds(ConstantValues.GameWindowStatusRefreshTime))
                .TakeUntil(_stayTopSubject.Where(on => !on));

            _stayTopSubject
                .DistinctUntilChanged()
                .Where(on => on && !MainWindowHandle.IsNull)
                .SelectMany(interval)
                .Subscribe(_ => User32.BringWindowToTop(MainWindowHandle));

            FrameworkElement? _parent = null;

            // TODO: Hide button when game window size changed
            gameWindowHooker.GamePosUpdated
                .Where(_ => _parent is not null)
                .Select(pos => (pos.Width, pos.Height))
                .DistinctUntilChanged()
                .Select(_ => Utils.IsGameForegroundFullscreen(gameDataService.MainWindowHandle))
                .Do(isFullscreen => _stayTopSubject.OnNext(isFullscreen))
                .Delay(TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime))
                .ObserveOnDispatcher()
                .Subscribe(_ => AssistiveButton.Margin = GetButtonEdgeMargin(ButtonSize, _touchPosition, _parent!));

            _mainWindowDataService.DpiSubject
                .Where(_ => _parent is not null)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.MinimumLagTime))
                .ObserveOnDispatcher()
                .Subscribe(_ => AssistiveButton.Margin = GetButtonEdgeMargin(ButtonSize, _touchPosition, _parent!));
            #endregion

            #region Surface
            // The diameter of button
            double _distance = 0;
            double _halfDistance = 0;
            double _oneThirdDistance = 0;
            double _twoThirdDistance = 0;

            void UpdateButtonProperties(double width)
            {
                _distance = width;
                _halfDistance = _distance / 2;
                _oneThirdDistance = _distance / 3;
                _twoThirdDistance = _oneThirdDistance * 2;
            }

            _mainWindowDataService.AssistiveTouchBigSizeSubject
                .Where(_ => _parent is not null)
                .Subscribe(_ =>
                {
                    UpdateButtonProperties(ButtonSize);
                    AssistiveButton.Margin = GetButtonEdgeMargin(ButtonSize, _touchPosition, _parent!);
                });
            #endregion

            #region Initialize
            AssistiveButton.Events().Loaded
                .Subscribe(_ =>
                {
                    if (Parent is not FrameworkElement tmp)
                    {
                        throw new InvalidOperationException("Control's parent must be FrameworkElement type");
                    }

                    _parent = tmp;

                    UpdateButtonProperties(ButtonSize);
                    AssistiveButton.Margin = GetButtonEdgeMargin(ButtonSize, _touchPosition, _parent);
                    AssistiveTouchFlyout.Placement = GetFlyoutPlacement(_touchPosition);
                });
            #endregion

            #region Opacity Adjust
            Point _lastPos;
            var _isMoving = false;
            BehaviorSubject<bool> tryTransparentizeSubj = new(true);
            tryTransparentizeSubj
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.AssistiveTouchOpacityChangedTimeout))
                .ObserveOnDispatcher()
                .Where(on => on && _isMoving == false && AssistiveTouchFlyout.IsOpen == false)
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
                _isMoving = true;
                tryTransparentizeSubj.OnNext(true);
                _lastPos = evt.GetPosition(_parent);
                _oldPos = _lastPos;
            });

            mouseMoveAndCheckEdge.Subscribe(evt =>
            {
                if (!_isMoving || _parent is null)
                    return;

                var pos = evt.GetPosition(_parent);
                // 相对左上坐标 + 新位置与旧位置的差值 = 新坐标
                var left = AssistiveButton.Margin.Left + pos.X - _lastPos.X;
                var top = AssistiveButton.Margin.Top + pos.Y - _lastPos.Y;

                // dummy problem UserControl would clip if Button get out of the bottom of window
                var workaround = 0.0;
                var outerBounds = top + ButtonSize;
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

            mouseRelease.Subscribe(evt =>
            {
                if (!_isMoving || _parent is null)
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
                var right = parentActualWidth - left - ButtonSize;
                // button 距离下边缘距离
                var bottom = parentActualHeight - top - ButtonSize;
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
                    top = parentActualHeight - ButtonSize - ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerLeft);
                }
                else if (right < _halfDistance && top < _twoThirdDistance) // top-right
                {
                    left = parentActualWidth - ButtonSize - ButtonSpace;
                    top = ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.UpperRight);
                }
                else if (right < _halfDistance && bottom < _twoThirdDistance) // bottom-right
                {
                    left = parentActualWidth - ButtonSize - ButtonSpace;
                    top = parentActualHeight - ButtonSize - ButtonSpace;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.LowerRight);
                }
                else if (top < _twoThirdDistance) // top
                {
                    left = curMarginLeft;
                    top = ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + (ButtonSize / 2)) / parentActualWidth;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Top, scale);
                }
                else if (bottom < _twoThirdDistance) // bottom
                {
                    left = curMarginLeft;
                    top = parentActualHeight - ButtonSize - ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + (ButtonSize / 2)) / parentActualWidth;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Bottom, scale);
                }
                else if (left + (ButtonSize / 2) < verticalMiddleLine) // left
                {
                    left = ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + (ButtonSize / 2)) / parentActualHeight;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Left, scale);
                }
                else // right
                {
                    left = parentActualWidth - ButtonSize - ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + ButtonSize / 2) / parentActualHeight;
                    _touchPosition = new AssistiveTouchPosition(TouchButtonCorner.Right, scale);
                }

                SmoothMoveAnimation(AssistiveButton, left, top);
                _isMoving = false;
                tryTransparentizeSubj.OnNext(true);
                AssistiveTouchFlyout.Placement = GetFlyoutPlacement(_touchPosition);
            });
            #endregion

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.AssistiveTouchTemplate,
                    v => v.AssistiveButton.Template)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.AssistiveTouchVisibility,
                    v => v.AssistiveButton.Visibility)
                    .DisposeWith(d);

                // Flyout Commands
                this.Bind(ViewModel,
                    vm => vm.LoseFocusIsOn,
                    v => v.LoseFocusToggle.IsChecked)
                    .DisposeWith(d);
                
                this.Bind(ViewModel,
                    vm => vm.IsTouchToMouse,
                    v => v.TouchConversionToggle.IsChecked)
                    .DisposeWith(d);

                this.WhenAnyObservable(x => x.ViewModel!.HideFlyoutSubj)
                    .Subscribe(_ => AssistiveTouchFlyout.Hide());

                this.BindCommand(ViewModel,
                    vm => vm.VolumeDown,
                    v => v.VolumeDown)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.VolumeUp,
                    v => v.VolumeUp)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.SwitchFullScreen,
                    v => v.FullScreenSwitcher)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.TaskbarNotifyArea,
                    v => v.ActionCenter)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.TaskView,
                    v => v.TaskView)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.ScreenShot,
                    v => v.ScreenShot)
                    .DisposeWith(d);

                // Custom apearrence: ButtonSpace, square to circle, size of the square 
            });
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

        private static FlyoutPlacementMode GetFlyoutPlacement(AssistiveTouchPosition _touchPosition) =>
            _touchPosition.Corner is
            TouchButtonCorner.Right or TouchButtonCorner.UpperRight or TouchButtonCorner.LowerRight
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

        private AppBarButton? _fullScreenButton;

        private void FullScreenButton_Loaded(object sender, RoutedEventArgs e)
        {
            var appbarButton = sender as AppBarButton;
            _fullScreenButton = appbarButton;
        }
    }
}