using System;
using System.Diagnostics;
using ErogeHelper.Common.Entities;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        IObservable<GameWindowPositionPacket> GamePosUpdated { get; }

        void SetGameWindowHook(Process process);

        void InvokeUpdatePosition();

        void InvokePositionAsMainFullscreen();

        GameWindowPositionPacket GetCurrentGamePosition();
    }
}
