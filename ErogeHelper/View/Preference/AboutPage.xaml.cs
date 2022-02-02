using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.ViewModel.Preference;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Preference;

public partial class AboutPage
{
    private new AboutViewModel ViewModel => base.ViewModel!;

    public AboutPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.AppVersion.Text)
                .BindTo(this, x => x.ViewModel.AppVersion);

            HandleActivation();

            this.OneWayBind(ViewModel,
                vm => vm.VersionBrushColor,
                v => v.VersionBorder.BorderBrush,
                color => color.ToNativeBrush()).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.VersionBrushColor,
                v => v.VersionForeground.Foreground,
                color => color.ToNativeBrush()).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.CanJumpRelease,
                v => v.VersionBorder.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.UpdateStatusTip,
                v => v.UpdateStatusTip.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.ShowUpdateButton,
                v => v.UpdateButton.Visibility,
                value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.Update,
                v => v.UpdateButton).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.CheckUpdate,
                v => v.PreviewUpdateButton,
                Observable.Return(true)).DisposeWith(d);

            this.BindInteraction(ViewModel,
               vm => vm.AppExit,
               context =>
               {
                   App.Terminate();
                   context.SetOutput(Unit.Default);
                   return Task.CompletedTask;
               }).DisposeWith(d);
        });
    }

    private void HandleActivation() => ViewModel.CheckUpdate.Execute(false).Subscribe();

    private void PreviewUpdateButtonOnClick(object sender, RoutedEventArgs e) =>
        ((Button)sender).SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
}
