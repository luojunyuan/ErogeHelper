using System.Reactive.Disposables;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class TTSPage
{
    public TTSPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.Voices,
                v => v.SupportVoices.ItemsSource).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.Play,
                v => v.Play).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.SelectedVoice,
                v => v.SupportVoices.SelectedItem).DisposeWith(d);
        });
    }
}
