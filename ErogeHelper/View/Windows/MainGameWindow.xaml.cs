using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Windows;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Windows;

public partial class MainGameWindow : IEnableLogger
{
    public MainGameWindow()
    {
        InitializeComponent();

        var keyDownDisposal = this.Events().PreviewKeyDown
            .Where(e => e.Key == Key.Tab)
            .Subscribe(e => e.Handled = true);

        var handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);

        ViewModel ??= DependencyResolver.GetService<MainGameViewModel>();

        ViewModel!.MainWindowHandle = handle;
        ViewModel.Dpi = WpfScreenHelper.Screen
                .FromHandle(handle.DangerousGetHandle())
                .ScaleFactor;

        this.Events().Loaded
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Loaded);

        this.Events().DpiChanged
            .Select(arg => arg.NewDpi.DpiScaleX)
            .InvokeCommand(this, x => x.ViewModel!.DpiChanged);

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

            keyDownDisposal.DisposeWith(d);
            ViewModel.DisposeWith(d);
            ViewModel.AssistiveTouchViewModel.DisposeWith(d);
        });
    }
}
