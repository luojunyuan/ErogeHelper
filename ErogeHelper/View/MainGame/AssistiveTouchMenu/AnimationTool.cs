using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

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
    public static TranslateTransform LeftTwoTransform(double distance) => new(distance * 2, 0);
    public static TranslateTransform RightOneTransform(double distance) => new(-distance, 0);
    public static TranslateTransform RightTwoTransform(double distance) => new(-distance * 2, 0);
    public static TranslateTransform BottomOneTransform(double distance) => new(0, -distance);
    public static TranslateTransform BottomTwoTransform(double distance) => new(0, -distance * 2);

    public static TranslateTransform LeftOneBottomOneTransform(double distance) => new(distance, -distance);
    public static TranslateTransform LeftOneBottomTwoTransform(double distance) => new(distance, -distance * 2);
    public static TranslateTransform LeftOneTopOneTransform(double distance) => new(distance, distance);
    public static TranslateTransform RightOneTopOneTransform(double distance) => new(-distance, distance);
    public static TranslateTransform RightOneBottomOneTransform(double distance) => new(-distance, -distance);
    public static TranslateTransform RightOneBottomTwoTransform(double distance) => new(-distance, -distance * 2);
    public static TranslateTransform RightTwoTopOneTransform(double distance) => new(-distance * 2, distance);

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

    public static void Fill(this IEnumerable<IMenuItemBackground> children, bool isBlock)
    {
        foreach (var control in children)
        {
            control.TransparentBackground(!isBlock);
        }
    }
}
