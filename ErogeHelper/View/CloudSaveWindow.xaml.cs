using ErogeHelper.Shared;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;

namespace ErogeHelper.View.Windows;

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
