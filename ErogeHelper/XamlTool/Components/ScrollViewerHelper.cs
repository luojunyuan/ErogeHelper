#pragma warning disable IDE0051 // No reference to GetDependencyProperty
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ErogeHelper.XamlTool.Components;

public static class ScrollViewerHelper
{
    #region IsAnimating DependencyProperty

    internal static readonly DependencyProperty IsAnimatingProperty =
        DependencyProperty.RegisterAttached(
            "IsAnimating",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false));

    /// <summary>Helper for getting <see cref="IsAnimatingProperty"/> from <paramref name="scrollViewer"/>.</summary>
    /// <param name="scrollViewer"><see cref="ScrollViewer"/> to read <see cref="IsAnimatingProperty"/> from.</param>
    /// <returns>IsAnimating property value.</returns>
    [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
    internal static bool GetIsAnimating(ScrollViewer scrollViewer) =>
        (bool)scrollViewer.GetValue(IsAnimatingProperty);

    /// <summary>Helper for setting <see cref="IsAnimatingProperty"/> on <paramref name="scrollViewer"/>.</summary>
    /// <param name="scrollViewer"><see cref="ScrollViewer"/> to set <see cref="IsAnimatingProperty"/> on.</param>
    /// <param name="value">IsAnimating property value.</param>
    internal static void SetIsAnimating(ScrollViewer scrollViewer, bool value) =>
        scrollViewer.SetValue(IsAnimatingProperty, value);

    #endregion

    #region CurrentVerticalOffset DependencyProperty

    internal static readonly DependencyProperty CurrentVerticalOffsetProperty =
        DependencyProperty.RegisterAttached("CurrentVerticalOffset",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(0.0, OnCurrentVerticalOffsetChanged));

    [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
    private static double GetCurrentVerticalOffset(ScrollViewer scrollViewer) =>
            (double)scrollViewer.GetValue(CurrentVerticalOffsetProperty);

    private static void SetCurrentVerticalOffset(ScrollViewer scrollViewer, double value) =>
        scrollViewer.SetValue(CurrentVerticalOffsetProperty, value);

    private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer ctl && e.NewValue is double v)
            ctl.ScrollToVerticalOffset(v);
    }

    #endregion

    #region CurrentHorizontalOffset DependencyProperty

    internal static readonly DependencyProperty CurrentHorizontalOffsetProperty =
        DependencyProperty.RegisterAttached("CurrentHorizontalOffset",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(0.0, OnCurrentHorizontalOffsetChanged));

    [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
    private static double GetCurrentHorizontalOffset(ScrollViewer scrollViewer) =>
            (double)scrollViewer.GetValue(CurrentHorizontalOffsetProperty);

    private static void SetCurrentHorizontalOffset(ScrollViewer scrollViewer, double value) =>
        scrollViewer.SetValue(CurrentHorizontalOffsetProperty, value);

    private static void OnCurrentHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer ctl && e.NewValue is double v)
            ctl.ScrollToHorizontalOffset(v);
    }

    #endregion

    internal static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;

        var isHorizontal = Keyboard.Modifiers == ModifierKeys.Shift;

        if (!isHorizontal)
        {
            if (!GetIsAnimating(scrollViewer))
                SetCurrentVerticalOffset(scrollViewer, scrollViewer.VerticalOffset);

            var totalVerticalOffset = Math.Min(Math.Max(0, scrollViewer.VerticalOffset - e.Delta), scrollViewer.ScrollableHeight);
            ScrollToVerticalOffset(scrollViewer, totalVerticalOffset);
        }
        else
        {
            if (!GetIsAnimating(scrollViewer))
                SetCurrentHorizontalOffset(scrollViewer, scrollViewer.HorizontalOffset);

            var totalHorizontalOffset = Math.Min(Math.Max(0, scrollViewer.HorizontalOffset - e.Delta), scrollViewer.ScrollableWidth);
            ScrollToHorizontalOffset(scrollViewer, totalHorizontalOffset);
        }
    }

    public static void ScrollToOffset(ScrollViewer scrollViewer, Orientation orientation, double offset, double duration = 500, IEasingFunction? easingFunction = null)
    {
        var animation = new DoubleAnimation(offset, TimeSpan.FromMilliseconds(duration));
        easingFunction ??= new CubicEase
        {
            EasingMode = EasingMode.EaseOut
        };
        animation.EasingFunction = easingFunction;
        animation.FillBehavior = FillBehavior.Stop;
        animation.Completed += (s, e1) =>
        {
            if (orientation == Orientation.Vertical)
                SetCurrentVerticalOffset(scrollViewer, offset);
            else
            {
                SetCurrentHorizontalOffset(scrollViewer, offset);
            }
            SetIsAnimating(scrollViewer, false);
        };
        SetIsAnimating(scrollViewer, true);

        scrollViewer.BeginAnimation(orientation == Orientation.Vertical ? CurrentVerticalOffsetProperty : CurrentHorizontalOffsetProperty, animation, HandoffBehavior.Compose);
    }

    public static void ScrollToVerticalOffset(
        ScrollViewer scrollViewer, double offset,
        double duration = 500, IEasingFunction? easingFunction = null) =>
        ScrollToOffset(scrollViewer, Orientation.Vertical, offset, duration, easingFunction);

    public static void ScrollToHorizontalOffset(
        ScrollViewer scrollViewer, double offset,
        double duration = 500, IEasingFunction? easingFunction = null) =>
        ScrollToOffset(scrollViewer, Orientation.Horizontal, offset, duration, easingFunction);
}
