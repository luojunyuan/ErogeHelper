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
        });
    }
}
