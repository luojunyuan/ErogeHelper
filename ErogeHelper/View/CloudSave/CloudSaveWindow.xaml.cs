using System.Reactive.Disposables;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel.CloudSave;
using ReactiveUI;

namespace ErogeHelper.View.CloudSave;

public partial class CloudSaveWindow
{
    public CloudSaveWindow()
    {
        InitializeComponent();
        ViewModel ??= DependencyResolver.GetService<CloudSaveViewModel>();

        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel,
                vm => vm.SetUNCPath,
                v => v.UNC).DisposeWith(d);
            
            this.OneWayBind(ViewModel,
                vm => vm.UNCDatabasePath,
                v => v.UNCPath.Text).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.SetRomingPath,
                v => v.SavedataRoming).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.SetDocumentsPath,
                v => v.SavedataDocuments).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.SetGameSaveFolderPath,
                v => v.SavedataGameFolder).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.GameSavedataPath,
                v => v.SavedataPath.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.CanEnable,
                v => v.CloudSaveSwitch.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.IsSwitchOn,
                v => v.CloudSaveSwitch.IsOn).DisposeWith(d);
        });
    }
}
