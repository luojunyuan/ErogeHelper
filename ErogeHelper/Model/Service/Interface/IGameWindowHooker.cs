using ErogeHelper.Common.Entity;
using System;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IGameWindowHooker
    {
        event Action<GameWindowPosition> GamePosArea;

        Task SetGameWindowHookAsync();

        /// <summary>
        /// Some games may change their handle when the window is switched full screen
        /// </summary>
        /// <returns></returns>
        void ResetWindowHandler();
    }
}