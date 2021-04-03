using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    public static class StaticXamlUiValues
    {
        public static readonly double MinWindowWidth = 400;
        public static readonly double MinButtonWidth = 90;
        public static readonly double ControllerWidth = 240;
        public static readonly double ExpandedModeThresholdWidth = 500;
        public static readonly double SidePaneLength = 180;
        public static readonly double AssistiveTouchSize = 55;
        public static readonly BitmapImage TransparentImage =
            Utils.LoadBitmapFromResource("assets/text_background/transparent.png");
        public static readonly BitmapImage AquaGreenImage =
            Utils.LoadBitmapFromResource("assets/text_background/aqua_green.png");
        public static readonly BitmapImage GreenImage = 
            Utils.LoadBitmapFromResource("assets/text_background/green.png");
        public static readonly BitmapImage PinkImage = 
            Utils.LoadBitmapFromResource("assets/text_background/pink.png");

        // ModernFlyouts
        // Any Controller width: 240
        // Buttons: WidthToContent
        // SidePaneLength: 225

        // System
        // Buttons: MinWidth 90, font between horizontal edge at least 15 pixel
        // SidePaneLength: 320
        // 下拉框: 230 or 280 and bigger
        // Extra web help info: 240

        // Expanded Change width (window width)
        // System 720 -> Frame min width 400 (window min 500) 
        // ModernFlyouts 800 -> 560 (350)
    }
}