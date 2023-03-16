using System.Reactive.Disposables;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.TextDisplay;

public partial class TextWindow : IEnableLogger
{
    public TextWindow()
    {
        InitializeComponent();
        var handle = State.TextWindowHandle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);

        ViewModel = new TextViewModel();

        this.WhenActivated(d =>
        {
            Disposable.Create(() => State.TextWindowHandle = nint.Zero).DisposeWith(d);
        });
    }
}
