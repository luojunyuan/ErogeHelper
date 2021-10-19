using System;
using System.Diagnostics;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Enums;
using ErogeHelper.Model.DataServices.Interface;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        IObservable<GameWindowPositionPacket> GamePosUpdated { get; }

        IObservable<WindowOperation> WindowOperationSubj { get; }

        void SetupGameWindowHook(Process process, IGameDataService? gameDataService = null);

        GameWindowPositionPacket InvokeUpdatePosition();
    }
}
