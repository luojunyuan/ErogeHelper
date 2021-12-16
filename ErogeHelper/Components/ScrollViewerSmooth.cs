using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErogeHelper.Components;

/// <summary>
/// Smooth move animation for ScrollViewer
/// </summary>
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
