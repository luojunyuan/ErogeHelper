using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.MainGame;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.MainGame;

public partial class MainGameWindow : IEnableLogger
{
    public MainGameWindow()
    {
        InitializeComponent();
        var handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);

        ViewModel ??= DependencyResolver.GetService<MainGameViewModel>();
        ViewModel.InitMainWindowHandle(handle);

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
               vm => vm.TouchToolBoxVisible,
               v => v.TouchToolBox.Visibility,
               value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);


            //this.OneWayBind(ViewModel,
            //   vm => vm.AssistiveTouchViewModel,
            //   v => v.AssistiveTouchHost.ViewModel).DisposeWith(d);
        });
    }

    private void HandleTabKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            e.Handled = true;
        }
    }

    private void MainGameWindowOnDpiChanged(object sender, DpiChangedEventArgs e) =>
        ViewModel!.UpdateDpi(e.NewDpi.DpiScaleX);
}
