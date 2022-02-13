using System.Drawing;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.TextDisplay;

public class AppendTextItemViewModel
{
    public string Text { get; init; } = string.Empty;

    public string ExtraInfo { get; init; } = "😺";

    [Reactive]
    public double FontSize { get; set; }

    [Reactive]
    public FontFamily? FontFamily { get; set; }
}
