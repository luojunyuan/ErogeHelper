using System;
using System.Diagnostics;
using ErogeHelper.Common.Entities;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        event EventHandler<GameWindowPositionEventArgs> GamePosChanged;

        void SetGameWindowHook(Process process);

        void InvokeUpdatePosition();
    }
}
