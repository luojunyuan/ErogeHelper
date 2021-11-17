using ErogeHelper.Functions;
using ErogeHelper.Share;
using ErogeHelper.ViewModel.Windows;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vanara.PInvoke;

namespace ErogeHelper.View.Windows
{
    public partial class MainGameWindow : IEnableLogger
    {
        public MainGameWindow()
        {
            InitializeComponent();

            ViewModel ??= DependencyResolver.GetService<MainGameViewModel>();

            var handle = WpfHelper.GetWpfWindowHandle(this);

            ViewModel.MainWindowHandle = handle;
                
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
                    .RegisterHandler(context => { Hide(); context.SetOutput(Unit.Default);}).DisposeWith(d);
                
                ViewModel.ShowMainWindow
                    .RegisterHandler(context => { Show(); context.SetOutput(Unit.Default);}).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Height,
                    v => v.Height).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Width,
                    v => v.Width).DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.Left,
                    v => v.Left).DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.Top,
                    v => v.Top).DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ClientAreaMargin,
                    v => v.ClientArea.Margin,
                    margin => new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom)).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.ShowEdgeTouchMask,
                    v => v.PreventFalseTouchMask.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed,
                    visibility => visibility == Visibility.Visible).DisposeWith(d);
                    

                this.Bind(ViewModel,
                    vm => vm.TouchToolBoxViewModel,
                    v => v.TouchToolBoxHost.ViewModel).DisposeWith(d);

                this.Bind(ViewModel,
                   vm => vm.AssistiveTouchViewModel,
                   v => v.AssistiveTouchHost.ViewModel).DisposeWith(d);
            });
        }
    }
}
