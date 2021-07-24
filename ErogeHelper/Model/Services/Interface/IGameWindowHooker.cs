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
        /// Some games may change their handle when the window is switched to fullscreen
        /// </summary>
        /// <returns></returns>
        void ResetWindowHandler();

        void InvokeUpdatePosition();
    }
}
