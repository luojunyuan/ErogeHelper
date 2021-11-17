namespace ErogeHelper.Share.Structs
{
    public readonly struct WindowPosition
    {
        public WindowPosition(double height, double width, double left, double top, Thickness clientArea)
        {
            Height = height;
            Width = width;
            Left = left;
            Top = top;
            ClientArea = clientArea;
        }

        public double Height { get; }
        public double Width { get; }
        public double Left { get; }
        public double Top { get; }
        public Thickness ClientArea { get; }

        public override string ToString() => $"({Left}, {Top}), width={Width} height={Height}";
    }
}

