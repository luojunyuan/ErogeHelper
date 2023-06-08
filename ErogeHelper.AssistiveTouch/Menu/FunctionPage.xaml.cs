using ErogeHelper.AssistiveTouch.Core;
using ErogeHelper.AssistiveTouch.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ErogeHelper.AssistiveTouch.Menu
{
    public partial class FunctionPage : Page, ITouchMenuPage
    {
        public event EventHandler<PageEventArgs>? PageChanged;

        public FunctionPage()
        {
            InitializeComponent();
            InitializeAnimation();
        }

        public void Show(double moveDistance)
        {
            SetCurrentValue(VisibilityProperty, Visibility.Visible);

            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

            var backTransform = AnimationTool.LeftOneTransform(moveDistance);
            Back.SetCurrentValue(RenderTransformProperty, backTransform);
            _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);

            _transitionInStoryboard.Begin();
        }

        public void Close()
        {
            XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);
            _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
            _transitionInStoryboard.Begin();
            _transitionInStoryboard.Seek(TouchButton.MenuTransistDuration);
        }

        private void BackOnClickEvent(object sender, EventArgs e) => PageChanged?.Invoke(this, new(TouchMenuPageTag.FunctionBack));

        private readonly Storyboard _transitionInStoryboard = new();
        private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

        private void InitializeAnimation()
        {
            AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

            AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);

            _transitionInStoryboard.Completed += (_, _) =>
            {
                XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

                if (!_transitionInStoryboard.AutoReverse)
                {
                    Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                }
                else
                {
                    _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                    SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    TouchMenuItem.ClickLocked = false;
                }
            };
        }

        private Border? _batteryInfo;
        private System.Timers.Timer? _batteryMonitor;
        private void BatteryOnToggledEvent(object sender, EventArgs e)
        {
            if (Battery.IsOn)
            {
                _batteryInfo = CreateBorder(out var dischargeRate, out var percentage, out var remain, out var averageDischargeRate, out var predict);
                _batteryMonitor = CreateTimer(dischargeRate, percentage, remain, averageDischargeRate, predict);
                _batteryMonitor.Enabled = true;
                ((Grid)(Application.Current.MainWindow.Content)).Children.Insert(0, _batteryInfo);
            }
            else
            {
                ((Grid)(Application.Current.MainWindow.Content)).Children.Remove(_batteryInfo);
                _batteryMonitor?.Dispose();
            }
        }

        private static System.Timers.Timer CreateTimer(TextBlock a, TextBlock b, TextBlock c, TextBlock d, TextBlock e)
        {
            var timer = new System.Timers.Timer
            {
                Interval = 1000
            };
            var totalSeconds = 0;
            var countRateAlteration = 0;
            var lastCapacity = 0;
            var currentCapacity = 0;
            var displayCapacity = 0;
            var currentRate = 0;
            var averageRate = 0;
            var totalEnerge = 0;
            int percent7 = BatteryInfo.GetBatteryInformation().FullChargeCapacity * 6 / 100;
            var fromCharging = false;
            timer.Elapsed += (s, evt) =>
            {
                if (SystemParameters.PowerLineStatus == PowerLineStatus.Online)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        a.Text = "Charging";
                        b.Text = c.Text = d.Text = e.Text = string.Empty;
                    });
                    countRateAlteration = 0;
                    lastCapacity = 0;
                    currentCapacity = 0;
                    displayCapacity = 0;
                    currentRate = 0;
                    averageRate = 0;
                    totalEnerge = 0;
                    totalSeconds = 0;
                    fromCharging = true;
                    return;
                }

                if (fromCharging)
                {
                    fromCharging = false;
                    timer.Stop();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        a.Text = "Recovering";
                        b.Text = c.Text = d.Text = e.Text = string.Empty;
                    });
                    Thread.Sleep(10000);
                    timer.Start();
                    return;
                }

                var info = BatteryInfo.GetBatteryInformation();

                currentCapacity = (int)info.CurrentCapacity;
                var newRate = info.DischargeRate;
                countRateAlteration = (newRate == currentRate) switch
                {
                    true => countRateAlteration + 1,
                    false => ((Func<int>)(() =>
                    {
                        currentRate = newRate;
                        return 0;
                    }))()
                };

                var perSecondDecrease = currentRate / 3600.0;
                displayCapacity = (currentCapacity == lastCapacity) switch
                {
                    true => displayCapacity += (int)perSecondDecrease,
                    false => ((Func<int>)(() =>
                    {
                        lastCapacity = currentCapacity;
                        return currentCapacity;
                    }))()
                };
                var remainSecond = (int)(displayCapacity / -perSecondDecrease);

                (averageRate, totalEnerge) = (averageRate == 0) switch
                {
                    true => (currentRate, -currentRate), // init
                    false => (-totalEnerge / totalSeconds, totalEnerge - currentRate)
                };

                var predict = (displayCapacity - percent7) / -(averageRate/3600.0);

                var aa = $"{Math.Round((double)-currentRate / 1000, 2)}Wh ({countRateAlteration}s)";
                var bb = (info.CurrentCapacity / (double)info.FullChargeCapacity).ToString("P0");
                var cc = $"{displayCapacity}mWh, {remainSecond / 60}m{remainSecond % 60}s";
                var dd = $"{Math.Round((double)-averageRate / 1000, 2)}Wh (average)";
                var ee = $"{totalSeconds / 60}:{totalSeconds % 60}-{(int)predict / 60}:{(int)predict % 60} (predict)";

                totalSeconds++;
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    a.Text = aa;
                    b.Text = bb;
                    c.Text = cc;
                    d.Text = dd;
                    e.Text = ee;
                });
            };

            return timer;
        }


        private static Border CreateBorder(
            out TextBlock dischargeRate,
            out TextBlock percentage,
            out TextBlock remain,
            out TextBlock averageDischargeRate,
            out TextBlock predict)
        {
            dischargeRate = new TextBlock() { FontSize = 24, Foreground = Brushes.White };
            percentage = new TextBlock() { FontSize = 24, Foreground = Brushes.White };
            remain = new TextBlock() { FontSize = 24, Foreground = Brushes.White };
            averageDischargeRate = new TextBlock() { FontSize = 24, Foreground = Brushes.White };
            predict = new TextBlock() { FontSize = 24, Foreground = Brushes.White };
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(dischargeRate);
            stackPanel.Children.Add(percentage);
            stackPanel.Children.Add(remain);
            stackPanel.Children.Add(averageDischargeRate);
            stackPanel.Children.Add(predict);
            return new Border()
            {
                Background = new SolidColorBrush() { Color = Colors.Black, Opacity = 0.6 },
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 224,
                Height = 160,
                Child = stackPanel
            };
        }
    }
}
