using System.Reactive.Disposables;
using System.Windows;
using ErogeHelper.Platform.XamlTool;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;

namespace ErogeHelper.View.TextDisplay;

public partial class FuriganaNone : IViewFor<FuriganaItemViewModel>
{
    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(FuriganaItemViewModel),
        typeof(FuriganaNone));

    public FuriganaItemViewModel? ViewModel
    {
        get => (FuriganaItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (FuriganaItemViewModel?)value);
    }
    #endregion

    public FuriganaNone(FuriganaItemViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.Text,
                v => v.TextItem.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.FontSize,
                v => v.TextItem.FontSize).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.BackgroundColor,
                v => v.MojiBackgroundColor.ImageSource,
                color => color.ToBitmapImage()).DisposeWith(d);
        });
    }
}
