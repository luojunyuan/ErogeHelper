namespace ErogeHelper.Shared.Structs;

public readonly record struct WindowPosition(double Height, double Width, double Left, double Top)
{
    public override string ToString() => $"({Left}, {Top}), width={Width} height={Height}";
}
