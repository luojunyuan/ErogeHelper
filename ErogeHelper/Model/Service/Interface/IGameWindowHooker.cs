using System.Collections.Generic;
using System.Diagnostics;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IGameWindowHooker
    {
        void SetGameWindowHook(IEnumerable<Process> processes);

        void ResetWindowHandler();
    }
}