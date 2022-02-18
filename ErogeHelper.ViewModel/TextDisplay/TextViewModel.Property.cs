using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.TextDisplay;

public partial class TextViewModel
{
    private string _currentText = string.Empty;
    private readonly CompositeDisposable _disposables = new();
    private readonly Subject<Unit> _showSubj = new();
    private readonly Subject<Unit> _hideSubj = new();
    private readonly ObservableCollection<FuriganaItemViewModel> _furiganaItemViewModel;
    private readonly ObservableCollection<AppendTextItemViewModel> _appendTextViewModel;

    public static HWND TextWindowHandle { get; set; } = IntPtr.Zero;

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    public IObservable<Unit> Show => _showSubj;

    public IObservable<Unit> Hide => _hideSubj;

    [Reactive]
    public double Left { get; private set; }

    [Reactive]
    public double Top { get; private set; }

    [Reactive]
    public double WindowWidth { get; private set; }

    [Reactive]
    public double WindowOpacity { get; private set; }

    public ObservableCollection<FuriganaItemViewModel> TextControlViewModel => _furiganaItemViewModel;

    public ObservableCollection<AppendTextItemViewModel> AppendTextControlViewModel => _appendTextViewModel;

    [ObservableAsProperty]
    public bool ShowFunctionNotEnableTip { get; }

    public ReactiveCommand<Unit, double> WindowWidthChanged { get; }

    public ReactiveCommand<Unit, double> WindowOpacityChanged { get; }

    public ReactiveCommand<Unit, Unit> Pronounce { get; }

    public ReactiveCommand<Unit, double> FontDecrease { get; }

    public ReactiveCommand<Unit, double> FontIncrease { get; }

    [Reactive]
    public bool EnableBlurBackground { get; private set; }

    [Reactive]
    private double FontSize { get; set; }
}
