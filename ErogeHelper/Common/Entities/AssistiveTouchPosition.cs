namespace ErogeHelper.Common.Entities
{
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

    public record AssistiveTouchPosition
    {
        public AssistiveTouchPosition(TouchButtonCorner corner, double scale = 0)
        {
            Corner = corner;
            Scale = scale;
        }

        public TouchButtonCorner Corner { get; init; }
        public double Scale { get; init; }
    }
}
