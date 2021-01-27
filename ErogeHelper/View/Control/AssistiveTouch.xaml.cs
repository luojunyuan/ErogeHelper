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

        public AssistiveTouch()
        {
            InitializeComponent();

            GameHooker.UpdateButtonPosEvent += (_) =>
            {
                SmoothMoveAnimation(buttonSpace, buttonSpace);
            };
        }
        private void RaiseMouseUpEventInCode()
        {
            int timestamp = new TimeSpan(DateTime.Now.Ticks).Milliseconds;
            MouseButton mouseButton = MouseButton.Left;

            var mouseUpEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, mouseButton)
            {
                RoutedEvent = PreviewMouseUpEvent,
                Source = this
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
                    Point pos = mouseEvent.GetPosition(parent);
                    newPos = pos;
                    double left = Margin.Left + pos.X - lastPos.X;
                    double top = Margin.Top + pos.Y - lastPos.Y;
                    // button 距离右边缘距离
                    double right = parent.ActualWidth - left - ActualWidth;
                    // button 距离下边缘距离
                    double bottom = parent.ActualHeight - top - ActualHeight;

                    double vertcalMiddelLine = parent.ActualHeight - ActualHeight - buttonSpace;
                    // 根据button所处屏幕位置来确定button之后应该动画移动到的位置
                    // FIXME: still bug in four corners, when button 中间卡在左窗口边缘
                    if (left < halfDistance && top < halfDistance) // button 距离左上角边距同时小于 distance
                    {
                        left = buttonSpace;
                        top = buttonSpace;
                    }
                    else if (left < halfDistance && bottom < halfDistance) // 左下
                    {
                        left = buttonSpace;
                        top = parent.ActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (right < halfDistance && top < halfDistance) // 右上
                    {
                        left = parent.ActualWidth - ActualWidth - buttonSpace;
                        top = buttonSpace;
                    }
                    else if (right < halfDistance && bottom < halfDistance) // 右下
                    {
                        left = parent.ActualWidth - ActualWidth - buttonSpace;
                        top = parent.ActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (top < twoThirdDistance) // 上
                    {
                        left = Margin.Left;
                        top = buttonSpace;
                    }
                    else if (bottom < twoThirdDistance) // 下
                    {
                        left = Margin.Left;
                        top = parent.ActualHeight - ActualHeight - buttonSpace;
                    }
                    else if (left < vertcalMiddelLine) // 左
                    {
                        left = buttonSpace;
                        top = Margin.Top;
                    }
                    else if (right < vertcalMiddelLine) // 右
                    {
                        left = parent.ActualWidth - ActualWidth - buttonSpace;
                        top = Margin.Top;
                    }
                    else
                    {
                        throw new InvalidOperationException("Should never happend!");
                    }

                    // 元素的某个属性，在开始值和结束值之间逐步增加，是一种线性插值的过程
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
