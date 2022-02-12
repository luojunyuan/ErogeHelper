using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel.HookConfig;
using ModernWpf.Controls;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.HookConfig;

public partial class ReiPatcherTipDialog : IViewFor<ReiPatcherTipViewModel>
{
    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(ReiPatcherTipViewModel),
        typeof(ReiPatcherTipDialog));

    public ReiPatcherTipViewModel? ViewModel
    {
        get => (ReiPatcherTipViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (ReiPatcherTipViewModel?)value);
    }
    #endregion

    public ReiPatcherTipDialog()
    {
        InitializeComponent();

        ViewModel = DependencyResolver.GetService<ReiPatcherTipViewModel>();

        this.WhenActivated(d => 
        {
            this.BindCommand(ViewModel,
                vm => vm.CopyAndRun,
                v => v.CopyAndRun).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.StepOneInfo,
                v => v.StepOneStatus.Text).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.StepOneColor,
                v => v.StepOneStatus.Foreground,
                color => color.ToNativeBrush()).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.ApplyConfig,
                v => v.ApplyConfig).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.StepTwoInfo,
                v => v.StepTwoStatus.Text).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.StepTwoColor,
                v => v.StepTwoStatus.Foreground,
                color => color.ToNativeBrush()).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Patch,
                v => v.Patch).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Restart,
                v => v.Restart).DisposeWith(d);
        });
    }
}
