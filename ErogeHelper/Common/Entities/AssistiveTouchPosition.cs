namespace ErogeHelper.Common.Entities;
public readonly record struct AssistiveTouchPosition(TouchButtonCorner Corner, double Scale = 0)
{
    public static readonly AssistiveTouchPosition Default = new(default, 0.5);
    public static readonly AssistiveTouchPosition UpperLeft = new(TouchButtonCorner.UpperLeft);
    public static readonly AssistiveTouchPosition LowerLeft = new(TouchButtonCorner.LowerLeft);
    public static readonly AssistiveTouchPosition UpperRight = new(TouchButtonCorner.UpperRight);
    public static readonly AssistiveTouchPosition LowerRight = new(TouchButtonCorner.LowerRight);
}

public enum TouchButtonCorner
{
    Left,
    Top,
    Right,
    Bottom,
    UpperLeft,
    UpperRight,
    LowerLeft,
    LowerRight
}
