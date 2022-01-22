using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.Platform.XamlTool
{
    internal static class XamlResource
    {
        public static ControlTemplate AssistiveTouchStyle
        {
            set => Application.Current.Resources["AssistiveTouchStyle"] = value;
        }

        public static double AssistiveTouchMenuMaxSize
        {
            set => Application.Current.Resources["AssistiveTouchMenuMaxSize"] = value;
        }

        public static double AssistiveTouchItemSize
        {
            set => Application.Current.Resources["AssistiveTouchItemSize"] = value;
        }

        public static double AssistiveTouchSize { get; }
            = (double)Application.Current.Resources["AssistiveTouchSize"];

        public static double AssistiveTouchBigSize { get; }
            = (double)Application.Current.Resources["BigAssistiveTouchSize"];

        public static ControlTemplate NormalAssistiveTouchTemplate { get; }
            = (ControlTemplate)Application.Current.Resources["NormalAssistiveTouchTemplate"];

        public static ControlTemplate BigAssistiveTouchTemplate { get; }
            = (ControlTemplate)Application.Current.Resources["BigAssistiveTouchTemplate"];

        public static double AssistiveTouchMenuNormalSize { get; }
            = (double)Application.Current.Resources["AssistiveTouchMenuNormalSize"];

        public static double AssistiveTouchMenuBiggerSize { get; }
            = (double)Application.Current.Resources["AssistiveTouchMenuBiggerSize"];

        public static double AssistiveTouchItemNormalSize { get; }
           = (double)Application.Current.Resources["AssistiveTouchItemNormalSize"];

        public static double AssistiveTouchItemBiggerSize { get; }
            = (double)Application.Current.Resources["AssistiveTouchItemBiggerSize"];
    }
}
