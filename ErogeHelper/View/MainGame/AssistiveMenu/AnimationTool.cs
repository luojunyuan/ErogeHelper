using System.Windows.Media.Animation;

namespace ErogeHelper.View.MainGame.AssistiveMenu;

internal static class AnimationTool
{
    public const string XProperty = "(UIElement.RenderTransform).(TranslateTransform.X)";
    public const string YProperty = "(UIElement.RenderTransform).(TranslateTransform.Y)";

    static AnimationTool()
    {
        FadeOutAnimation.Freeze();
        FadeInAnimation.Freeze();
    }

    public static DoubleAnimation CreateFadeInAnimation() => FadeInAnimation;

    public static DoubleAnimation CreateFadeOutAnimation() => FadeOutAnimation;

    public static DoubleAnimation CreateTransformMoveToZeroAnimation() => TransformMoveToZeroAnimation;
    
    public static DoubleAnimation CreateSizeChangeAnimation() => SizeChangeAnimation;

    public static DoubleAnimation FadeOutAnimation => new()
    {
        From = 1.0,
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
    };
    private static DoubleAnimation FadeInAnimation => new()
    {
        From = 0.0,
        To = 1.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration),
    };
    private static DoubleAnimation TransformMoveToZeroAnimation => new()
    {
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
    };
    private static DoubleAnimation SizeChangeAnimation => new()
    {
        EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut },
        Duration = TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration)
    };
}
