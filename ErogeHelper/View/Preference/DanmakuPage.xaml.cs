using System.Reactive.Disposables;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class DanmakuPage
{
    public DanmakuPage()
    {
        InitializeComponent();

        // 没有Id 无法同步，新建面板来配置，post gamesetting
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.DanmakuEnable,
                v => v.DanmakuSwitch.IsOn).DisposeWith(d);
        });
    }
}
