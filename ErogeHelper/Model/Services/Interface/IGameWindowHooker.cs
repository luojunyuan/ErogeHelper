using System;
using System.Diagnostics;
using ErogeHelper.Common.Entities;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IGameWindowHooker
    {
        event EventHandler<GameWindowPositionEventArgs> GamePosChanged;

        void SetGameWindowHook(Process process);

        /// <summary>
        /// Some games may recreate their windows
        /// </summary>
        /// <returns></returns>
        void ResetWindowHandler();

        void InvokeUpdatePosition();
    }
}
