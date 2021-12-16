namespace ErogeHelper.Shared.Structs;

public readonly struct Thickness
{
    public readonly double Left;
    public readonly double Top;
    public readonly double Right;
    public readonly double Bottom;

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
}
