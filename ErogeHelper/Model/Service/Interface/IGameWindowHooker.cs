using ErogeHelper.Common.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
