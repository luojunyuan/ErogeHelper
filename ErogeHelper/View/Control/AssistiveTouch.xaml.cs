using ErogeHelper.Common.Helper;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// FloatButton.xaml 的交互逻辑
    /// </summary>
    public partial class AssistiveTouch : Button
    {
        public new event EventHandler? ClickEvent;
        private FrameworkElement parent = null!;

        private bool move = false;

        private const double opacityValue = 0.4;
        private const double opacityNormal = 1;
        private const double buttonSpace = 2;
        // positioning button absorb field
        private double distance;
        private double halfDistance;
        private double oneThirdDistance;
        private double twoThirdDistance;

        private Point lastPos;

        private Point newPos;
        private Point oldPos;

        private bool isFromUpdateButtonPosEvent = false;
        private int newGameViewHeight;
        private int newGameViewWidth;

        public AssistiveTouch()
        {
            InitializeComponent();

            GameHooker.UpdateButtonPosEvent += (_, height, width) =>
            {
                move = true;
                isFromUpdateButtonPosEvent = true;
                newGameViewHeight = height;
                newGameViewWidth = width;
                RaiseMouseUpEventInCode();
            };
        }
        private void RaiseMouseUpEventInCode()
        {
            int timestamp = new TimeSpan(DateTime.Now.Ticks).Milliseconds;
            MouseButton mouseButton = MouseButton.Left;

            var mouseUpEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, mouseButton)
            {
                RoutedEvent = PreviewMouseUpEvent,
                Source = this,
            };

            RaiseEvent(mouseUpEvent);
        }

        private void RegisterParentPreviewEvent()
        {
            parent.PreviewMouseMove += (_, mouseEvent) =>
            {
                if (move)
                {
                    // 获取相对于左上角的坐标
                    Point pos = mouseEvent.GetPosition(parent);
                    // 相对左上坐标 + 新位置与旧位置的差值 = 新坐标
                    double left = Margin.Left + pos.X - lastPos.X;
                    double top = Margin.Top + pos.Y - lastPos.Y;
                    // 依靠Margin来实现移动的效果
                    Margin = new Thickness(left, top, 0, 0);

                    lastPos = pos;
                    if (left < -oneThirdDistance || top < -oneThirdDistance ||
                        left > parent.ActualWidth - twoThirdDistance || top > parent.ActualHeight - twoThirdDistance)
                    {
                        RaiseMouseUpEventInCode();
                    }
                }
            };

            parent.PreviewMouseUp += (_, mouseEvent) =>
            {
                if (move)
                {
                    double left, top, right, bottom, vertcalMiddelLine,
                        parentActualHeight, parentActualWidth;

                    if (isFromUpdateButtonPosEvent)
                    {
                        parentActualWidth = newGameViewWidth;
                        parentActualHeight = newGameViewHeight;

                        isFromUpdateButtonPosEvent = false;
                    }
                    else
                    {
                        Point pos = mouseEvent.GetPosition(parent);
                        newPos = pos;
                        parentActualHeight = parent.ActualHeight;
                        parentActualWidth = parent.ActualWidth;
                    }

                    left = Margin.Left + newPos.X - lastPos.X;
                    top = Margin.Top + newPos.Y - lastPos.Y;
                    // button 距离右边缘距离
                    right = parentActualWidth - left - ActualWidth;
                    // button 距离下边缘距离
                    bottom = parentActualHeight - top - ActualHeight;
                    vertcalMiddelLine = parentActualHeight - ActualHeight - buttonSpace;

                    //Log.Info($"鼠标位置 {newPos.X} {newPos.Y}");
                    //Log.Info($"释放点与四边距离 {left} {top} {right} {bottom}");

                    // 根据button所处屏幕位置来确定button之后应该动画移动到的位置
                    // FIXME: still bug in four corners, when button 中间卡在左窗口边缘
                    if (left < halfDistance && top < twoThirdDistance) // button 距离左上角边距同时小于 distance
                    {
                        left = buttonSpace;
                        top = buttonSpace;
                    }
                    else if (left < halfDistance && bottom < twoThirdDistance) // 左下
                    {
                        left = buttonSpace;
                        top = parentActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (right < halfDistance && top < twoThirdDistance) // 右上
                    {
                        left = parentActualWidth - ActualWidth - buttonSpace;
                        top = buttonSpace;
                    }
                    else if (right < halfDistance && bottom < twoThirdDistance) // 右下
                    {
                        left = parentActualWidth - ActualWidth - buttonSpace;
                        top = parentActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (top < twoThirdDistance) // 上
                    {
                        left = Margin.Left;
                        top = buttonSpace;
                    }
                    else if (bottom < twoThirdDistance) // 下
                    {
                        left = Margin.Left;
                        top = parentActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (left < vertcalMiddelLine) // 左
                    {
                        left = buttonSpace;
                        top = Margin.Top;
                    }
                    else if (right < vertcalMiddelLine) // 右
                    {
                        left = parentActualWidth - ActualWidth - buttonSpace;
                        top = Margin.Top;
                    }
                    else
                    {
                        throw new InvalidOperationException("Should never happend!");
                    }

                    // 元素的某个属性，在开始值和结束值之间逐步增加，是一种线性插值的过程
                    //Log.Info($"最终button移动位置 {left} {top}");
                    SmoothMoveAnimation(left, top);
                    move = false;
                }
            };
        }

        private void SmoothMoveAnimation(double left, double top)
        {
            ThicknessAnimation marginAnimation = new ThicknessAnimation
            {
                From = Margin,
                To = new Thickness(left, top, 0, 0),
                Duration = TimeSpan.FromMilliseconds(300)
            };

            Storyboard story = new Storyboard
            {
                FillBehavior = FillBehavior.Stop
            };
            story.Children.Add(marginAnimation);
            Storyboard.SetTargetName(marginAnimation, nameof(FloatButton));
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath("(0)", Border.MarginProperty));

            story.Begin(this);

            Margin = new Thickness(left, top, 0, 0);
        }

        private void FloatButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (Parent is null && Parent is not FrameworkElement)
            {
                throw new InvalidOperationException("Control's parent must be FrameworkElement type");
            }
            parent = (Parent as FrameworkElement)!;

            distance = Width;
            halfDistance = distance / 2;
            oneThirdDistance = distance / 3;
            twoThirdDistance = oneThirdDistance * 2;

            RegisterParentPreviewEvent();

            //double left = parent.ActualWidth - ActualWidth - distanceNew;
            //double top = parent.ActualHeight - ActualHeight - distanceNew;
            Margin = new Thickness(buttonSpace, buttonSpace, 0, 0);

            // for opacity the button
            FloatButton_Click(FloatButton, new RoutedEventArgs());
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FloatButton.Opacity = opacityNormal;
            move = true;
            lastPos = e.GetPosition(parent);
            oldPos = lastPos;
            //Log.Info($"lastPos and oldPos {lastPos.X} {lastPos.Y}");
        }

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        // After mouse released
        private void FloatButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            tokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = tokenSource.Token;

            if (newPos.Equals(oldPos))
            {
                // Fire MouseClickEvent, no use for me
                ClickEvent?.Invoke(sender, e);
            }
            else
            {
                // Disable flyout popup
                e.Handled = true;
            }

            Task.Run(async () =>
            {
                await Task.Delay(5000).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (move == false && !cancelToken.IsCancellationRequested)
                    {
                        FloatButton.Opacity = opacityValue;
                    }
                });
            }, cancelToken);
        }

        // XXX: Seems didn't work in this Controls, maybe only work with window
        #region Disable White Point by Touch
        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);
            Cursor = Cursors.None;
        }
        protected override void OnPreviewTouchMove(TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);
            Cursor = Cursors.None;
        }
        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);
            Cursor = Cursors.Arrow;
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (e.StylusDevice == null)
                Cursor = Cursors.Arrow;
        }
        #endregion
    }
}
