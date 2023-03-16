using ErogeHelper.Common.Definitions;
using ErogeHelper.Common.Entities;
using System.Diagnostics;
using System.Reactive;

namespace ErogeHelper.Model.Services;

public interface IGameWindowHooker
{
    IObservable<WindowPosition> GamePosUpdated { get; }

    IObservable<WindowSizeDelta> GameSizeUpdated { get; }

    IObservable<ViewOperation> WhenViewOperated { get; }
    
    IObservable<Unit> BringKeyboardWindowTopDataFlow { get; }

    int SetupGameWindowHook(Process process);

    WindowPosition InvokeUpdatePosition();
}
