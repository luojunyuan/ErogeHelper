using System.Reactive.Disposables;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class DanmakuPage
{
    public DanmakuPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel,
                vm => vm.DanmakuEnable,
                v => v.DanmakuSwitch.IsOn).DisposeWith(d);
        });
    }
}
