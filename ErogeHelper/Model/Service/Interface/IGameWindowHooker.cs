using System;
using System.Diagnostics;
using ErogeHelper.Common.Entity;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IGameWindowHooker
    {
        event EventHandler<GameWindowPositionEventArgs> GamePosChanged;

        IntPtr GameRealHwnd { get; }

        void SetGameWindowHook(Process process);

        /// <summary>
        /// Some games may change their handle when the window is switched to fullscreen
        /// </summary>
        /// <returns></returns>
        void ResetWindowHandler();

        void InvokeUpdatePosition();
    }
}
