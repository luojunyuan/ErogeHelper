using System.Windows;
using ErogeHelper.ViewModel;

namespace ErogeHelper.Platform.MISC;

public class DisablePointerMessage : Window
{
    /// <summary>
    /// WPF automatically upgraded to the point message for some reason, 
    /// Creating a new window in advance can prevent this behavior.
    /// </summary>
    public static void Apply()
    {
        var workaround = new DisablePointerMessage();
        workaround.Show();
        workaround.Close();
    }

    private DisablePointerMessage()
    {
        Left = -32000;
        Top = -32000;
        Width = 0;
        Height = 0;
        ShowInTaskbar = false;
        var handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);
    }
}
