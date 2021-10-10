using ErogeHelper.Common.Enums;

namespace ErogeHelper.Common.Entities
{
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
