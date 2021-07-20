using System.Windows;

namespace ErogeHelper.Common.Entities
{
    public class GameWindowPositionEventArgs
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
