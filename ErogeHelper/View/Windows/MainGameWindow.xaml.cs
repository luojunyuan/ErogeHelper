using ErogeHelper.Common;
using ErogeHelper.ViewModel.Windows;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Vanara.PInvoke;

namespace ErogeHelper.View.Windows
{
    public partial class MainGameWindow : IEnableLogger
    {
        private readonly HWND _handle;

        public MainGameWindow(MainGameViewModel? gameViewModel = null)
        {
            InitializeComponent();

            ViewModel = gameViewModel ?? DependencyInject.GetService<MainGameViewModel>();

            _handle = Utils.GetWpfWindowHandle(this);
            this.WhenAnyValue(x => x._handle)
                .BindTo(this, x => x.ViewModel!.MainWindowHandle);

            this.Events().Loaded
                .Select(_ => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.Loaded);

            this.Events().DpiChanged
                .Select(arg => arg.NewDpi.DpiScaleX)
                .InvokeCommand(this, x => x.ViewModel!.DpiChanged);

            this.WhenActivated(d =>
            {
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
                    vm => vm.ClientAreaMargin,
                    v => v.ClientArea.Margin).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.AssistiveTouchViewModel,
                    v => v.AssistiveTouchHost.ViewModel).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.UseEdgeTouchMask,
                    v => v.PreventFalseTouchMask.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed,
                    visibility => visibility == Visibility.Visible).DisposeWith(d);
            });
        }
    }
}
