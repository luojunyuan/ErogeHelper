using System;
using System.Windows;

namespace ErogeHelper.Common.Entities
{
    public class GameWindowPositionPacket : EventArgs
    {
        public GameWindowPositionPacket(double height, double width, double left, double top, Thickness clientArea)
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

        public override string ToString()
        {
            return $"({Left}, {Top}), width={Width} height={Height}";
        }
    }
}
