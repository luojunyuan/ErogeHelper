using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.TextDisplay;

public partial class TextWindow : IEnableLogger
{
    public TextWindow()
    {
        InitializeComponent();
        HwndTools.HideWindowInAltTab(TextViewModel.TextWindowHandle = WpfHelper.GetWpfWindowHandle(this));

        ViewModel = DependencyResolver.GetService<TextViewModel>();
        Width = ViewModel.WindowWidth;

        var disposables = new CompositeDisposable();
        this.WhenAnyObservable(x => x.ViewModel!.Show)
            .Subscribe(_ => Show())
            .DisposeWith(disposables);

        this.WhenAnyObservable(x => x.ViewModel!.Hide)
            .Subscribe(_ => Hide())
            .DisposeWith(disposables);

        this.Events().Loaded
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Loaded)
            .DisposeWith(disposables);

        MouseEnter += (_, _) => DragBar.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        MouseLeave += (_, _) => DragBar.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
        ControlCenterFlyout.Opened += (_, _) => DragBar.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        ControlCenterFlyout.Events().Closed
            .Where(_ => !IsMouseOver)
            .Subscribe(_ => DragBar.SetCurrentValue(VisibilityProperty, Visibility.Hidden))
            .DisposeWith(disposables);
        BlurSwitch.Toggled += (_, args) =>
        {
            if (BlurSwitch.IsOn)
            {
                OpacitySlider.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.MinimumProperty, 0.02);
                if (OpacitySlider.Value < OpacitySlider.Minimum)
                {
                    ViewModel.WindowOpacityChanged.Execute(OpacitySlider.Minimum).Subscribe();
                }
            }
            else
            {
                OpacitySlider.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.MinimumProperty, 0.0);
                if (OpacitySlider.Value == 0.02)
                {
                    ViewModel.WindowOpacityChanged.Execute(OpacitySlider.Minimum).Subscribe();
                }
            }
        };


        this.WhenActivated(d =>
        {
            ViewModel.DisposeWith(d);
            disposables.DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.WindowWidth,
                v => v.Width).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.Left,
                v => v.Left).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.Top,
                v => v.Top).DisposeWith(d);


            this.OneWayBind(ViewModel,
                vm => vm.WindowWidth,
                v => v.WidthSlider.Value).DisposeWith(d);
            WidthSlider.Events().ValueChanged
                .Select(arg => arg.NewValue)
                .InvokeCommand(this, x => x.ViewModel!.WindowWidthChanged).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.OpacitySlider.Value).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.BackgroundOpacity.Opacity).DisposeWith(d);
            OpacitySlider.Events().ValueChanged
                .Select(arg => arg.NewValue)
                .InvokeCommand(this, x => x.ViewModel!.WindowOpacityChanged).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Pronounce,
                v => v.PronounceButton).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.EnableBlurBackground,
                v => v.BlurSwitch.IsOn).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.TextControlViewModel,
                v => v.TextItemsControl.ItemsSource).DisposeWith(d);
        });
    }

    private void DragBarOnLeftButtonDown(object sender, RoutedEventArgs args) => DragMove();

    private void DragBarOnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        => ControlCenterFlyout.ShowAt(sender as FrameworkElement);
}
