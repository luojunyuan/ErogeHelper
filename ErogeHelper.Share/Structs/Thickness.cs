namespace ErogeHelper.Share.Structs
{
    public struct Thickness
    {
        public Thickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(double uniformLength)
        {
            Left = Top = Right = Bottom = uniformLength;
        }

        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
    }
}

