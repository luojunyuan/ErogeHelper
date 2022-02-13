using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows;
using ErogeHelper.Platform;
using ErogeHelper.View.TextDisplay;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;
using System.Reactive;

namespace ErogeHelper.View.Preference;

public partial class GeneralPage
{
    public GeneralPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel,
                vm => vm.UseBigSizeAssistiveTouch,
                v => v.BigSizeAssistiveTouch.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.HideTextWindow,
                v => v.HideTextWindow.IsOn).DisposeWith(d);
            this.WhenAnyObservable(x => x.ViewModel!.ShowOrCloseTextWindow)
                .Subscribe(InteractTextWindow).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseEdgeTouchMask,
                v => v.EdgeTouchMask.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.StartupInject,
                v => v.NotStartupInject.IsChecked,
                x => !x, x => (bool)!x!).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseDPIDpiCompatibility,
                v => v.DpiCompatibility.IsChecked).DisposeWith(d);
        });
    }

    private static void InteractTextWindow(Unit _)
    {
        var textWindow = Application.Current.Windows.Cast<Window>()
            .Where(w => w.GetType() == typeof(TextWindow))
            .FirstOrDefault();
        if (textWindow != null)
        {
            textWindow.Close();
        }
        else
        {
            DI.ShowView<TextViewModel>();
        }
    }

    private void ForceGCButtonOnClick(object sender, RoutedEventArgs e) => GC.Collect();
}
