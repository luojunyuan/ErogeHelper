using System;
using System.Diagnostics;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Enums;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        IObservable<GameWindowPositionPacket> GamePosUpdated { get; }

        IObservable<WindowOperation> WindowOperationSubj { get; }

        void SetGameWindowHook(Process process);

        GameWindowPositionPacket InvokeUpdatePosition();
    }
}
