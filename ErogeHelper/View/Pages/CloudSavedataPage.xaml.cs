using ReactiveUI;
using System.Reactive.Disposables;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// SavedataSyncWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CloudSavedataPage
    {
        public CloudSavedataPage()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel,
                    vm => vm.OpenCloudEditDialog,
                    v => v.CloudEditButton)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.CloudSwitchCanBeSet,
                    v => v.CloudSaveSwitch.IsEnabled)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.CloudSwitchIsOn,
                    v => v.CloudSaveSwitch.IsOn)
                    .DisposeWith(d);
            });
        }
    }
}
