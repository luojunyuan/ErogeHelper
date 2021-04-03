using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErogeHelper.Common.Function;

namespace ErogeHelper.View.Control
{
    // XXX: When I reference this, my xaml designer could not resolve StaticResource or DynamicResource UI style anymore
    public class ScrollViewerSmooth : ScrollViewer
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Style == null && ReadLocalValue(StyleProperty) == DependencyProperty.UnsetValue)
            {
                SetResourceReference(StyleProperty, typeof(ScrollViewer));
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled) { return; }
            ScrollViewerHelper.OnMouseWheel(this, e);
            e.Handled = true;
        }
    }
}