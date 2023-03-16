using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;
using ErogeHelper.Function;
using ErogeHelper.View.TextDisplay;
using ReactiveUI;

namespace ErogeHelper.View.Modern.Preference;

public partial class GeneralPage
{
    public GeneralPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.TouchSizeSource,
                v => v.SelectedTouchSize.ItemsSource).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.TouchSize,
                v => v.SelectedTouchSize.SelectedItem).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.ShowTextWindow,
                v => v.ShowTextWindow.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseEdgeTouchMask,
                v => v.EdgeTouchMask.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.StartupInject,
                v => v.InjectAtStartup.IsChecked).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseDPIDpiCompatibility,
                v => v.DpiCompatibility.IsChecked).DisposeWith(d);

            ViewModel.WhenAnyValue(x => x.ShowTextWindow)
                .Skip(1)
                .Subscribe(v =>
                {
                    if (v) new TextWindow().Show();
                    else ((Window)HwndSource.FromHwnd(State.TextWindowHandle).RootVisual).Close();
                });


            this.Bind(ViewModel,
                vm => vm.UseMagSmoothing,
                v => v.MagSmoothing.IsOn).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.MagDataString,
                v => v.MagData.Text).DisposeWith(d);
        });
    }
}
