using ErogeHelper.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// FloatButton.xaml 的交互逻辑
    /// </summary>
    public partial class AssistiveTouch : Button
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AssistiveTouch));

        public new event EventHandler? ClickEvent;
        private FrameworkElement parent = null!;

        private bool move = false;

        const double distance = 50; // aka button width
        const double halfDistance = distance / 2;
        const double buttonSpace = 2;

        private Point lastPos;

        private Point newPos;
        private Point oldPos;

        public AssistiveTouch()
        {
            InitializeComponent();

            GameHooker.UpdateButtonPosEvent += (_) =>
            {
                Margin = new Thickness(buttonSpace, buttonSpace, 0, 0);
            };
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
                }
            };

            parent.PreviewMouseUp += (_, mouseEvent) =>
            {
                if (move)
                {
                    move = false;

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
                    // FIXME: bug when button 中间卡在左窗口边缘
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
                    //else if (top < distance && left > distance && right > distance) // 上
                    else if (top < distance) // 上
                    {
                        left = Margin.Left;
                        top = buttonSpace;
                    }
                    //else if (bottom < distance && left > distance && right > distance) // 下
                    else if (bottom < distance) // 下
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
                        log.Error("Should never happend!");
                    }

                    // 元素的某个属性，在开始值和结束值之间逐步增加，是一种线性插值的过程
                    ThicknessAnimation marginAnimation = new ThicknessAnimation
                    {
                        From = Margin,
                        To = new Thickness(left, top, 0, 0),
                        Duration = TimeSpan.FromMilliseconds(300)
                    };

                    Storyboard story = new Storyboard();
                    story.FillBehavior = FillBehavior.Stop;
                    story.Children.Add(marginAnimation);
                    Storyboard.SetTargetName(marginAnimation, nameof(FloatButton));
                    Storyboard.SetTargetProperty(marginAnimation, new PropertyPath("(0)", Border.MarginProperty));

                    story.Begin(this);

                    Margin = new Thickness(left, top, 0, 0);

                    // TODO: After 5 seconds, Transform to opacity
                }
            };
        }

        private void FloatButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (Parent is null && Parent is not FrameworkElement)
            {
                throw new InvalidOperationException("Control's parent must be FrameworkElement type");
            }
            parent = (Parent as FrameworkElement)!;

            RegisterParentPreviewEvent();

            //double left = parent.ActualWidth - ActualWidth - distanceNew;
            //double top = parent.ActualHeight - ActualHeight - distanceNew;
            Margin = new Thickness(buttonSpace, buttonSpace, 0, 0);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: Transform template to deep
            move = true;
            lastPos = e.GetPosition(parent);
            oldPos = lastPos;
        }

        private void FloatButton_Click(object sender, RoutedEventArgs e)
        {
            // After mouse released
            if (newPos.Equals(oldPos))
            {
                if (ClickEvent != null)
                {
                    log.Info("Click event fire!");
                    ClickEvent(sender, e);
                }
            }
            else
            {
                // Disable flyout popup
                e.Handled = true;
            }
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
