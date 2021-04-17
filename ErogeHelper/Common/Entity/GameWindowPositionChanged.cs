namespace ErogeHelper.Common.Entity
{
    public class GameWindowPositionChanged
    {
        public GameWindowPositionChanged(double horizontalChange, double verticalChange)
        {
            HorizontalChange = horizontalChange;
            VerticalChange = verticalChange;
        }

        public double HorizontalChange { get; }
        public double VerticalChange { get; }
    }
}