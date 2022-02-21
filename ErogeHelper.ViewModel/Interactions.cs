using System.Reactive;
using ReactiveUI;

namespace ErogeHelper.ViewModel;

public static class Interactions
{
    public static Interaction<string, bool> MessageBoxConfirm { get; } = new();

    public static Interaction<Unit, string> MeCabDictFileSelectDialog { get; } = new();

    public static Interaction<string, bool> ContentDialog { get; } = new();

    public static Interaction<(string RootFolder, string Description), string> FolderBrowserDialog { get; } = new();
}
