using ErogeHelper.Common.Entities;
using System.Windows;

namespace ErogeHelper.Common.Contracts
{
    public class DefaultValues
    {
        public static readonly AssistiveTouchPosition TouchPosition = new(TouchButtonCorner.Left, 0.5);

        public static readonly double AssistiveTouchSize = (double)Application.Current.Resources["AssistiveTouchSize"];
        public static readonly double AssistiveTouchBigSize = (double)Application.Current.Resources["BigAssistiveTouchSize"];
    }
}
