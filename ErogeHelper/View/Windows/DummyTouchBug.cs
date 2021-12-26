using System.Windows;
using ErogeHelper.Platform;
using ErogeHelper.ViewModel;

namespace ErogeHelper.View.Windows;

public class DummyTouchBug : Window
{
    /// <summary>
    /// This fix the problem of ScrollViewer can not receive Manipulation events caused by unknown reason
    /// </summary>
    public static void Fix()
    {
        var dummyBug = new DummyTouchBug();
        dummyBug.Show();
        dummyBug.Close();
    }

    private DummyTouchBug()
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
