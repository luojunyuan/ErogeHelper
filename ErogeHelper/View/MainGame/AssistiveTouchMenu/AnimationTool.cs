﻿using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.MainGame.AssistiveMenu;

internal static class AnimationTool
{
    public const string XProperty = "(UIElement.RenderTransform).(TranslateTransform.X)";
    public const string YProperty = "(UIElement.RenderTransform).(TranslateTransform.Y)";

    public static DoubleAnimation FadeOutAnimation => new()
    {
        From = 1.0,
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        FillBehavior = FillBehavior.Stop,
    };

    public static DoubleAnimation FadeInAnimation => new()
    {
        From = 0.0,
        To = 1.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        FillBehavior = FillBehavior.Stop,
    };

    public static TranslateTransform ZeroTransform => new(0, 0);
    public static TranslateTransform LeftOneTransform(double distance) => new(distance, 0);
    public static TranslateTransform RightOneTransform(double distance) => new(-distance, 0);
    public static TranslateTransform BottomOneTransform(double distance) => new(0, -distance);
    public static TranslateTransform BottomOneLeftOneTransform(double distance) => new(distance, -distance);
    public static TranslateTransform BottomOneRightOneTransform(double distance) => new(-distance, -distance);
    public static TranslateTransform BottomTwoRightOneTransform(double distance) => new(-distance, -distance * 2);

    public static DoubleAnimation TransformMoveToZeroAnimation => new()
    {
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
        FillBehavior = FillBehavior.Stop,
    };

    public static DoubleAnimation TransformMoveToTargetAnimation => new()
    {
        From = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
        FillBehavior = FillBehavior.Stop,
    };

    public static DoubleAnimation SizeChangeAnimation => new()
    {
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
        FillBehavior = FillBehavior.Stop,
    };

    public static void FillBackground(this IEnumerable<MenuItemControl> children, bool fill)
    {
        foreach (var control in children)
        {
            control.TransparenceBackground(!fill);
        }
    }
}