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
    };

    public static DoubleAnimation FadeInAnimation => new()
    {
        From = 0.0,
        To = 1.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
    };

    public static DoubleAnimation TransformMoveToZeroAnimation => new()
    {
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
    };

    public static DoubleAnimation TransformMoveToTargetAnimation => new()
    {
        From = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
    };

    public static DoubleAnimation SizeChangeAnimation => new()
    {
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
    };
}
