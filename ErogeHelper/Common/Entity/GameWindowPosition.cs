using System.Windows;

namespace ErogeHelper.Common.Entity
{
    public class GameWindowPosition
    {
        public double Height;
        public double Width;
        public double Left;
        public double Top;
        public Thickness ClientArea;

        public override string ToString()
        {
            return $"({Left}, {Top}), width={Width} height={Height}";
        }
    }
}