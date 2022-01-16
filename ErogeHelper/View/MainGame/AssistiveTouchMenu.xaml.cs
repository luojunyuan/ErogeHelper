using System.Windows;
using ErogeHelper.Platform;
using ErogeHelper.ViewModel.Preference;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveTouchMenu
{
    private const double MaxSizeOfMenu = 300;
    private const int EndureEdgeHeight = 30;

    public AssistiveTouchMenu()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            var parent = Parent as FrameworkElement
                ?? throw new InvalidOperationException("Control's parent must be FrameworkElement type");

            parent.SizeChanged += ResizeMenu;
        };
    }

    private void ResizeMenu(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged && e.NewSize.Height > 30)
        {
            if (e.NewSize.Height > EndureEdgeHeight + MaxSizeOfMenu)
            {
                SetCurrentValue(HeightProperty, MaxSizeOfMenu);
                SetCurrentValue(WidthProperty, MaxSizeOfMenu);
            }
            else
            {
                SetCurrentValue(HeightProperty, e.NewSize.Height - 30);
                SetCurrentValue(WidthProperty, e.NewSize.Height - 30);
            }
        }
    }

    private void PreferenceOnClickEvent(object sender, EventArgs e) => DI.ShowView<PreferenceViewModel>();
}
