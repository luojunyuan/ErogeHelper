using System;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.Common.Enum;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Common.Selector
{
    public class TextTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? OutLineDefaultTemplate { get; set; }
        public DataTemplate? OutLineBottomTemplate { get; set; }
        public DataTemplate? OutLineTopTemplate { get; set; }
        public DataTemplate? OutLineVerticalTemplate { get; set; }
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            // 可以通过container找keyName，也可以通过绑定的template直接返回
            if (container is FrameworkElement && item is SingleTextItem textItem)
            {
                return textItem.TextTemplateType switch
                {
                    TextTemplateType.OutLineDefault => OutLineDefaultTemplate,
                    TextTemplateType.OutLineKanaTop => OutLineTopTemplate,
                    TextTemplateType.OutLineKanaBottom => OutLineBottomTemplate,
                    TextTemplateType.OutLineVertical => OutLineVerticalTemplate,
                    _ => throw new ArgumentOutOfRangeException(nameof(textItem.TextTemplateType), @"Invalid")
                };
            }
            return null;
        }
    }
}