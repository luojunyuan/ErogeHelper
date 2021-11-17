using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Share.Enums;
using ErogeHelper.Share.Structs;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        IObservable<WindowPosition> GamePosUpdated { get; }

        IObservable<ViewOperation> WhenViewOperated { get; }

        void SetupGameWindowHook(Process process, IGameDataService? gameDataService, IScheduler scheduler);

        WindowPosition InvokeUpdatePosition();
    }
}
