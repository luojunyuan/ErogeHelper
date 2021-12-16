using System.Reactive;
using ReactiveUI;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel;

public static class Interactions
{
    public static Interaction<HWND, bool> CheckGameFullscreen { get; } = new();

    public static Interaction<Unit, Unit> TerminateApp { get; } = new();

    public static Interaction<string, bool> MessageBoxConfirm { get; } = new();

    public static Interaction<Unit, string> MeCabDictFileSelectDialog { get; } = new();

    public static Interaction<string, bool> ContentDialog { get; } = new();
}
