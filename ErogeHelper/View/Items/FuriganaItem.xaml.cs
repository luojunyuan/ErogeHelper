using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.XamlTool;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Items;

public partial class FuriganaItem
{
    public FuriganaItem()
    {
        InitializeComponent();

        var layoutUpdatedEvent = this.Events()
            .LayoutUpdated
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.LayoutUpdated);

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.Kana,
                v => v.Kana.Text).DisposeWith(d);

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

            layoutUpdatedEvent.DisposeWith(d);
        });
    }
}
