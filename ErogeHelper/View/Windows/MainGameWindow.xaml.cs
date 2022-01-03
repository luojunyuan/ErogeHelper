using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.View.Controllers;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Controllers;
using ErogeHelper.ViewModel.Windows;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Windows;

public partial class MainGameWindow : IEnableLogger
{
    private AssistiveTouch? _assistivetouch;

    public MainGameWindow()
    {
        InitializeComponent();

        var handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);

        ViewModel ??= DependencyResolver.GetService<MainGameViewModel>();

        ViewModel.MainWindowHandle = handle;

        this.Events().Loaded
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Loaded);

        this.WhenActivated(d =>
        {
            ViewModel.HideMainWindow
                .RegisterHandler(context => { Hide(); context.SetOutput(Unit.Default); }).DisposeWith(d);

            ViewModel.ShowMainWindow
                .RegisterHandler(context => { Show(); context.SetOutput(Unit.Default); }).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Height,
                v => v.Height).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Width,
                v => v.Width).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Left,
                v => v.Left).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Top,
                v => v.Top).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.ShowEdgeTouchMask,
                v => v.PreventFalseTouchMask.Visibility,
                value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);


            this.OneWayBind(ViewModel,
                vm => vm.TouchToolBoxViewModel,
                v => v.TouchToolBoxHost.ViewModel).DisposeWith(d);

            this.OneWayBind(ViewModel,
               vm => vm.AssistiveTouchViewModel,
               v => v.AssistiveTouchHost.ViewModel).DisposeWith(d);
            this.WhenAnyValue(x => (AssistiveTouch?)x.AssistiveTouchHost.Content)
                .WhereNotNull()
                .Subscribe(assistivetouch =>
                {
                    _assistivetouch = assistivetouch;
                    assistivetouch.Clicked += AssistiveTouchClicked;
                }).DisposeWith(d);

            ViewModel.DisposeWith(d);
        });

        //AssistiveTouchMenuHost.PreviewMouseLeftButtonUp += HideAssistiveTouchMenu;
        Deactivated += MainGameWindow_Deactivated; ;
    }

    private void AssistiveTouchClicked(double buttonSize, Thickness buttonMargin)
    {
        AssistiveTouchAnimation.AnimatiedBorder.Margin = buttonMargin;
        AssistiveTouchAnimation.AnimatiedBorder.Visibility = Visibility.Visible;
        _assistivetouch.Visibility = Visibility.Collapsed;
        var relativeLocation = AssistiveTouchMenuHost.MenuBase.TranslatePoint(default, this);
        var tmp = new Thickness(relativeLocation.X, relativeLocation.Y, 0, 0);
        AssistiveTouchAnimation.BeginAnimation(tmp);
        //AssistiveTouchMenuHost.Visibility = Visibility.Visible;
        //AssistiveTouchAnimation.AnimatiedBorder.Visibility = Visibility.Collapsed;
    }
    // 每一个页面都定义一个back animation 或 storyboard，在失去焦点时侯判断是哪个页面调那个
    // 左上右下 功能 设备 返回(小白点图标) 偏好设置

    private void MainGameWindow_Deactivated(object? sender, EventArgs e)
    {
        _assistivetouch.Visibility = Visibility.Visible;
        AssistiveTouchMenuHost.Visibility = Visibility.Collapsed;
    }

    private void HideAssistiveTouchMenu(object sender, RoutedEventArgs e)
    {
        _assistivetouch.Visibility = Visibility.Visible;
        AssistiveTouchMenuHost.Visibility = Visibility.Collapsed;
    }

    private void HandleTabKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            e.Handled = true;
        }
    }

    private void MainGameWindowOnDpiChanged(object sender, DpiChangedEventArgs e) => 
        AmbiantContext.UpdateDpi(e.NewDpi.DpiScaleX);
}
