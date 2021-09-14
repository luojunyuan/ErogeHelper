using System;
using System.Windows;

namespace ErogeHelper.Common.Entities
{
    public record GameWindowPositionPacket
    {
        public GameWindowPositionPacket(double height, double width, double left, double top, Thickness clientArea)
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
        public Thickness ClientArea { get; init; }

        public override string ToString()
        {
            return $"({Left}, {Top}), width={Width} height={Height}";
        }
    }
}
