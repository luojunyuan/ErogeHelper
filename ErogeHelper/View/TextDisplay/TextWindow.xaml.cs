using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ErogeHelper.Platform.MISC;
using ErogeHelper.Platform.XamlTool.Components;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.TextDisplay;
using ModernWpf.Controls;
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
        Panel.BlurSwitch.Toggled += (_, args) =>
        {
            if (Panel.BlurSwitch.IsOn)
            {
                Panel.OpacitySlider.SetCurrentValue(RangeBase.MinimumProperty, 0.02);
                if (Panel.OpacitySlider.Value < Panel.OpacitySlider.Minimum)
                {
                    ViewModel.WindowOpacityChanged.Execute(Unit.Default).Subscribe();
                }
            }
            else
            {
                Panel.OpacitySlider.SetCurrentValue(RangeBase.MinimumProperty, 0.0);
                if (Panel.OpacitySlider.Value == 0.02)
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
                vm => vm.WindowWidth,
                v => v.Panel.WidthSlider.Value).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.WindowWidthChanged,
                v => v.Panel.WidthSlider).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.Panel.OpacitySlider.Value).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.WindowOpacity,
                v => v.BackgroundOpacity.Opacity).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.WindowOpacityChanged,
                v => v.Panel.OpacitySlider).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Pronounce,
                v => v.Panel.PronounceButton).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.FontDecrease,
                v => v.Panel.FontDecrease).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.FontIncrease,
                v => v.Panel.FontIncrease).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.EnableBlurBackground,
                v => v.Panel.BlurSwitch.IsOn).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.ShowFunctionNotEnableTip,
                v => v.FunctionNotEnableTip.Visibility,
                value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.TextAlignmentCenter,
                v => v.Panel.CurrentTextAlignSymbol.Symbol,
                center => center ? Symbol.AlignCenter : Symbol.AlignLeft,
                symbol => symbol == Symbol.AlignCenter).DisposeWith(d);
            this.WhenAnyValue(x => x.Panel.CurrentTextAlignSymbol.Symbol)
                .Skip(1)
                .Select(symbol => symbol == Symbol.AlignLeft ? HorizontalAlignment.Left : HorizontalAlignment.Center)
                .Subscribe(align => 
                    _textitemsPanel!.HorizontalContentAlignment = _appendTextsPanel!.HorizontalAlignment = align)
                .DisposeWith(d);


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

    private AlignableWrapPanel? _textitemsPanel;
    private void TextItemsPanelOnLoaded(object sender, RoutedEventArgs e)
    {
        _textitemsPanel = sender as AlignableWrapPanel;
        _textitemsPanel!.HorizontalContentAlignment = ViewModel!.TextAlignmentCenter ? 
            HorizontalAlignment.Center : HorizontalAlignment.Left;
    }

    private StackPanel? _appendTextsPanel;
    private void AppendTextsPanelOnLoaded(object sender, RoutedEventArgs e)
    {
        _appendTextsPanel = sender as StackPanel;
        _appendTextsPanel!.HorizontalAlignment = ViewModel!.TextAlignmentCenter ?
            HorizontalAlignment.Center : HorizontalAlignment.Left;
    }
}
