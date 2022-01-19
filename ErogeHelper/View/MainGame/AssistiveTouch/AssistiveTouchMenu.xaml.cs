using System.Windows;
using System.Windows.Navigation;
using Splat;

namespace ErogeHelper.View.MainGame;

public partial class AssistiveTouchMenu : IEnableLogger
{
    private const double MaxSizeOfMenu = 300;
    private const int EndureEdgeHeight = 30;

    public event EventHandler? Closed;

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

    public bool IsOpen { get; private set; }

    public void Show()
    {
        IsOpen = true;
        SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }

    public void Hide()
    {
        IsOpen = false;
        SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        Closed?.Invoke(this, new());
        while (MenuContent.CanGoBack)
        {
            MenuContent.GoBack();
        }
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

    // https://paulstovell.com/wpf-navigation/
    private void MenuContentOnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        if (e.Content is MenuMainPage menuMainPage)
        {
        }
        else if (e.Content is MenuDevicePage menuDevicePage)
        {
            //var sb = e.ExtraData as Storyboard;
            //sb.Begin();
            menuDevicePage.DeviceStoryboard.Begin();
            // back to the right position
            //menuDevicePage.volumeDownTransform.SetCurrentValue(System.Windows.Media.TranslateTransform.XProperty, (double)0);
            // 开始动画后，设置各个控件TranslateTransform的移动位置
            this.Log().Debug("menuDevicePage");
        }
        else
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
            }
        }
        //var ta = new ThicknessAnimation();
        //ta.Duration = TimeSpan.FromSeconds(0.3);
        //ta.DecelerationRatio = 0.7;
        //ta.To = new Thickness(0, 0, 0, 0);
        //if (e.NavigationMode == NavigationMode.New)
        //{
        //    ta.From = new Thickness(500, 0, 0, 0);
        //}
        //else if (e.NavigationMode == NavigationMode.Back)
        //{
        //    ta.From = new Thickness(0, 0, 500, 0);
        //}
        //(e.Content as Page).BeginAnimation(MarginProperty, ta);
    }
}
