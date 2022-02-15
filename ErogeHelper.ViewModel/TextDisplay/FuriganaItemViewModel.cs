using System.Drawing;
using ErogeHelper.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.TextDisplay;

public class FuriganaItemViewModel : ReactiveObject
{
    [Reactive]
    public string Text { get; set; } = string.Empty;

    [Reactive]
    public string Kana { get; set; } = string.Empty;

    [Reactive]
    public double FontSize { get; set; }

    [Reactive]
    public Color BackgroundColor { get; set; }
}
