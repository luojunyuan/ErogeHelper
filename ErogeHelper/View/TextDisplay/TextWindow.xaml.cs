using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.TextDisplay;
using ModernWpf.Controls.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.TextDisplay;

public partial class TextWindow : IEnableLogger
{
    public TextWindow(TextViewModel? textViewModel = null)
    {
        InitializeComponent();
        HwndTools.HideWindowInAltTab(TextViewModel.TextWindowHandle = WpfHelper.GetWpfWindowHandle(this));

        ViewModel = textViewModel ?? DependencyResolver.GetService<TextViewModel>();
        Width = ViewModel.WindowWidth;

        var disposables = new CompositeDisposable();
        this.Events().Loaded
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Loaded)
            .DisposeWith(disposables);

        MouseEnter += (_, _) => DragBar.Visibility = ControlButton.Visibility = Visibility.Visible;
        MouseLeave += (_, _) => DragBar.Visibility = ControlButton.Visibility = Visibility.Hidden;
        ControlCenterFlyout.Opened += (_, _) => DragBar.Visibility = ControlButton.Visibility = Visibility.Visible;
        ControlCenterFlyout.Events().Closed
            .Do(_ => ControlCenterFlyout.SetCurrentValue(
                FlyoutBase.PlacementProperty, FlyoutPlacementMode.TopEdgeAlignedRight))
            .Where(_ => !IsMouseOver)
            .Subscribe(_ => DragBar.Visibility = ControlButton.Visibility = Visibility.Hidden)
            .DisposeWith(disposables);
        // TODO: Refactor? (After test on win7
        BlurSwitch.Toggled += (_, args) =>
        {
            if (BlurSwitch.IsOn)
            {
                OpacitySlider.SetCurrentValue(RangeBase.MinimumProperty, 0.02);
                if (OpacitySlider.Value < OpacitySlider.Minimum)
                {
                    ViewModel.WindowOpacityChanged.Execute(Unit.Default).Subscribe();
                }
            }
            else
            {
                OpacitySlider.SetCurrentValue(RangeBase.MinimumProperty, 0.0);
                if (OpacitySlider.Value == 0.02)
                {
                    ViewModel.WindowOpacityChanged.Execute(Unit.Default).Subscribe();
                }
            }
        };


        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext).DisposeWith(d);

            ViewModel.DisposeWith(d);
            disposables.DisposeWith(d);

            this.WhenAnyObservable(x => x.ViewModel!.Show)
                .Subscribe(_ => Show()).DisposeWith(d);
            this.WhenAnyObservable(x => x.ViewModel!.Hide)
                .Subscribe(_ => Hide()).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.Left,
                v => v.Left).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.Top,
                v => v.Top).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.WindowWidth,
                v => v.Width).DisposeWith(d);


            this.Bind(ViewModel,
                vm => vm.WindowScale,
                v => v.WidthSlider.Value).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.WindowWidthChanged,
                v => v.WidthSlider).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.OpacitySlider.Value).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.BackgroundOpacity.Opacity).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.WindowOpacityChanged,
                v => v.OpacitySlider).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Pronounce,
                v => v.PronounceButton).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.FontDecrease,
                v => v.FontDecrease).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.FontIncrease,
                v => v.FontIncrease).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.EnableBlurBackground,
                v => v.BlurSwitch.IsOn).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.ShowFunctionNotEnableTip,
                v => v.FunctionNotEnableTip.Visibility,
                value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);


            this.OneWayBind(ViewModel,
                vm => vm.TextControlViewModel,
                v => v.TextItemsControl.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.AppendTextControlViewModel,
                v => v.AppendTextsControl.ItemsSource).DisposeWith(d);
        });
    }

    private void DragBarOnLeftButtonDown(object sender, RoutedEventArgs args) => DragMove();

    private void DragBarOnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        ControlCenterFlyout.SetCurrentValue(FlyoutBase.PlacementProperty, FlyoutPlacementMode.TopEdgeAlignedLeft);
        ControlCenterFlyout.ShowAt(sender as FrameworkElement);
    }
}
