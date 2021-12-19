using System.Reactive.Disposables;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Windows;

public partial class HookWindow : IEnableLogger
{
    public HookWindow()
    {
        InitializeComponent();

        ViewModel ??= DependencyResolver.GetService<HookViewModel>();

        this.WhenActivated(d =>
        {
            ViewModel.DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.CurrentInUseHookName,
                v => v.CurrentInUseHook.Content).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.CurrentInUseHookColor,
                v => v.CurrentInUseHook.Foreground,
                color => color.ToNativeBrush()).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.ConsoleInfo,
                v => v.ConsoleInfo.Text).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.ReInject,
                v => v.ReInject).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.OpenHCodeDialog,
                v => v.HCodeButton).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.HookEngineNames,
                v => v.HookThreadsComboBox.ItemsSource).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.SelectedHookEngine,
                v => v.HookThreadsComboBox.SelectedItem).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.RemoveHook,
                v => v.RemoveHookButton).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.HookThreadItems,
                v => v.HookThreadItems.ItemsSource).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Submit,
                v => v.SubmitButton).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.HCodeViewModel,
                v => v.HcodeDialogHost.ViewModel).DisposeWith(d);
        });
    }
}
