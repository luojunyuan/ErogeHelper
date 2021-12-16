namespace ErogeHelper.Shared.Entities;

public class AssistiveTouchPosition
{
    public static readonly AssistiveTouchPosition Default = new(TouchButtonCorner.Left, 0.5);
    public static readonly AssistiveTouchPosition UpperLeft = new(TouchButtonCorner.UpperLeft);
    public static readonly AssistiveTouchPosition LowerLeft = new(TouchButtonCorner.LowerLeft);
    public static readonly AssistiveTouchPosition UpperRight = new(TouchButtonCorner.UpperRight);
    public static readonly AssistiveTouchPosition LowerRight = new(TouchButtonCorner.LowerRight);

    public AssistiveTouchPosition(TouchButtonCorner corner, double scale = 0)
    {
        Corner = corner;
        Scale = scale;
    }

    public TouchButtonCorner Corner { get; set; }

    public double Scale { get; set; }
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
