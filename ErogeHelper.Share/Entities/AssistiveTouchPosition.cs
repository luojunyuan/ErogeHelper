using ErogeHelper.Share.Enums;

namespace ErogeHelper.Share.Entities
{
    public record AssistiveTouchPosition
    {
        public AssistiveTouchPosition(TouchButtonCorner corner, double scale = 0)
        {
            Corner = corner;
            Scale = scale;
        }

        public TouchButtonCorner Corner { get; }
        public double Scale { get;}
    }
}
