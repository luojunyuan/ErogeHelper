namespace ErogeHelper.Common.Entities
{
    public record GameWindowPositionPacket
    {
        public GameWindowPositionPacket(double height, double width, double left, double top, System.Windows.Thickness clientArea)
        {
            Height = height;
            Width = width;
            Left = left;
            Top = top;
            ClientArea = clientArea;
        }

        public double Height { get; init; }
        public double Width { get; init; }
        public double Left { get; init; }
        public double Top { get; init; }
        public System.Windows.Thickness ClientArea { get; init; }

        public override string ToString() => $"({Left}, {Top}), width={Width} height={Height}";
    }
}
