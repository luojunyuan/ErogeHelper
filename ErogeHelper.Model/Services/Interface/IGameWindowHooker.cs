using System.Diagnostics;
using System.Reactive.Concurrency;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Structs;

namespace ErogeHelper.Model.Services.Interface;

public interface IGameWindowHooker
{
    IObservable<WindowPosition> GamePosUpdated { get; }

    IObservable<WindowPositionChange> GamePosChanged { get; }

    IObservable<ViewOperation> WhenViewOperated { get; }

    void SetupGameWindowHook(Process process, IGameDataService? gameDataService, IScheduler scheduler);

    WindowPosition InvokeUpdatePosition();
}
