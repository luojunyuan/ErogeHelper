using ErogeHelper.ViewModel.Preference;
using ReactiveUI;

namespace ErogeHelper.View.Modern.Preference;

public partial class AboutPage
{
    private new AboutViewModel ViewModel => base.ViewModel!;

    public AboutPage()
    {
        InitializeComponent();

        this.WhenActivated(d => { });
    }
}
