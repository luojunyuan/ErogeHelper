using ErogeHelper.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErogeHelper.View.Control
{
    // XXX: When I reference this, my xaml designer could not resolve StaticResource or DynamicResource UI style anymore
    class ScrollViewerSmooth : ScrollViewer
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
            ScrollViewerHelperEx.OnMouseWheel(this, e);
            e.Handled = true;
        }
    }
}
