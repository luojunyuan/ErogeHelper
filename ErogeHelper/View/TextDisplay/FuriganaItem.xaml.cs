using System.Reactive.Disposables;
using ErogeHelper.Platform.XamlTool;
using ReactiveUI;

namespace ErogeHelper.View.TextDisplay;

public partial class FuriganaItem
{
    public FuriganaItem()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            //this.OneWayBind(ViewModel,
            //    vm => vm.Kana,
            //    v => v.Kana.Text).DisposeWith(d);

            //this.OneWayBind(ViewModel,
            //    vm => vm.Text,
            //    v => v.TextItem.Text).DisposeWith(d);

            //this.OneWayBind(ViewModel,
            //    vm => vm.FontSize,
            //    v => v.TextItem.FontSize).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.BackgroundColor,
                v => v.MojiBackgroundColor.ImageSource,
                color => color.ToBitmapImage()).DisposeWith(d);
        });
    }
}
